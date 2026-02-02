using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Notification_Application.Models;
using System.Text.Json;

namespace Notification_Application.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class FormApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public FormApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/FormApi/render/{embedCode}
    [HttpGet("render/{embedCode}")]
    public async Task<IActionResult> RenderForm(string embedCode)
    {
        var form = await _context.WebsiteForms
            .Include(f => f.Fields.OrderBy(field => field.Order))
            .Include(f => f.Tenant)
            .FirstOrDefaultAsync(f => f.EmbedCode == embedCode && f.Status == FormStatus.Published);

        if (form == null)
        {
            return NotFound(new { error = "Form not found or not published" });
        }

        // Track view
        form.Views++;
        
        // Update daily analytics
        var today = DateTime.UtcNow.Date;
        var analytics = await _context.FormAnalytics
            .FirstOrDefaultAsync(a => a.FormId == form.Id && a.Date == today);

        if (analytics == null)
        {
            analytics = new FormAnalytics
            {
                FormId = form.Id,
                Date = today
            };
            _context.FormAnalytics.Add(analytics);
        }

        analytics.Views++;

        // Detect device
        var userAgent = Request.Headers["User-Agent"].ToString().ToLower();
        if (userAgent.Contains("mobile"))
            analytics.MobileViews++;
        else if (userAgent.Contains("tablet"))
            analytics.TabletViews++;
        else
            analytics.DesktopViews++;

        await _context.SaveChangesAsync();

        // Return form data
        var response = new
        {
            id = form.Id,
            name = form.Name,
            style = form.Style.ToString().ToLower(),
            theme = form.Theme,
            settings = new
            {
                backgroundColor = form.BackgroundColor,
                textColor = form.TextColor,
                buttonColor = form.ButtonColor,
                buttonTextColor = form.ButtonTextColor,
                borderColor = form.BorderColor,
                borderRadius = form.BorderRadius,
                fontFamily = form.FontFamily,
                fontSize = form.FontSize,
                customCss = form.CustomCss
            },
            content = new
            {
                headerText = form.HeaderText,
                subheaderText = form.SubheaderText,
                submitButtonText = form.SubmitButtonText,
                footerText = form.FooterText,
                showPoweredBy = form.ShowPoweredBy
            },
            fields = form.Fields.Where(f => !f.IsHidden).Select(f => new
            {
                id = f.Id,
                name = f.FieldName,
                label = f.Label,
                placeholder = f.Placeholder,
                helpText = f.HelpText,
                type = f.FieldType.ToString().ToLower(),
                required = f.IsRequired,
                minLength = f.MinLength,
                maxLength = f.MaxLength,
                validationPattern = f.ValidationPattern,
                validationMessage = f.ValidationMessage,
                options = !string.IsNullOrEmpty(f.Options) ? JsonSerializer.Deserialize<List<string>>(f.Options) : null,
                allowOther = f.AllowOther,
                hasConditionalLogic = f.HasConditionalLogic,
                conditionalLogic = f.ConditionalLogic,
                width = f.Width,
                defaultValue = f.DefaultValue,
                preFillFromUrl = f.PreFillFromUrl
            }),
            enableHoneypot = form.EnableHoneypot,
            enableRecaptcha = form.EnableRecaptcha,
            recaptchaSiteKey = form.RecaptchaSiteKey
        };

        return Ok(response);
    }

    // GET: api/FormApi/standalone/{embedCode}
    [HttpGet("standalone/{embedCode}")]
    public async Task<IActionResult> StandaloneForm(string embedCode)
    {
        var form = await _context.WebsiteForms
            .Include(f => f.Fields.OrderBy(field => field.Order))
            .Include(f => f.Tenant)
            .FirstOrDefaultAsync(f => f.EmbedCode == embedCode && f.Status == FormStatus.Published);

        if (form == null)
        {
            return NotFound("Form not found or not published");
        }

        var baseUrl = form.Tenant?.PublicHostUrl ?? $"{Request.Scheme}://{Request.Host}";

        // Build standalone HTML page
        var html = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <title>{System.Web.HttpUtility.HtmlEncode(form.Name)}</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{
            font-family: {form.FontFamily ?? "-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif"};
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }}
        #optinyeti-form-{embedCode} {{
            width: 100%;
            max-width: 520px;
        }}
    </style>
</head>
<body>
    <div id=""optinyeti-form-{embedCode}""></div>
    <script src=""{baseUrl}/api/FormApi/embed.js"" data-form=""{embedCode}"" data-container=""optinyeti-form-{embedCode}""></script>
</body>
</html>";

        return Content(html, "text/html");
    }

    // POST: api/FormApi/submit/{embedCode}
    [HttpPost("submit/{embedCode}")]
    public async Task<IActionResult> SubmitForm(string embedCode, [FromBody] JsonElement formData)
    {
        try
        {
            var form = await _context.WebsiteForms
                .Include(f => f.Fields)
                .Include(f => f.Tenant)
                .FirstOrDefaultAsync(f => f.EmbedCode == embedCode && f.Status == FormStatus.Published);

            if (form == null)
            {
                return NotFound(new { error = "Form not found or not published" });
            }

            // Check honeypot
            if (form.EnableHoneypot)
            {
                if (formData.TryGetProperty("_hp_field", out var honeypot) && 
                    !string.IsNullOrEmpty(honeypot.GetString()))
                {
                    // Honeypot triggered - likely spam
                    return Ok(new { success = true }); // Return success to fool bots
                }
            }

            // Create submission
            var submission = new FormSubmission
            {
                FormId = form.Id,
                TenantId = form.TenantId,
                Data = formData.GetRawText(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString(),
                Referrer = Request.Headers["Referer"].ToString(),
                SubmittedAt = DateTime.UtcNow
            };

        // Extract UTM parameters
        if (formData.TryGetProperty("_utm_source", out var utmSource))
            submission.UtmSource = utmSource.GetString();
        if (formData.TryGetProperty("_utm_medium", out var utmMedium))
            submission.UtmMedium = utmMedium.GetString();
        if (formData.TryGetProperty("_utm_campaign", out var utmCampaign))
            submission.UtmCampaign = utmCampaign.GetString();
        if (formData.TryGetProperty("_page_url", out var pageUrl))
            submission.PageUrl = pageUrl.GetString();

        // Create lead if enabled
        if (form.CreateLead)
        {
            var lead = new Lead
            {
                TenantId = form.TenantId,
                Source = form.LeadSource ?? "Website Form",
                LeadSource = form.LeadSource ?? "Website Form",
                CapturedAt = DateTime.UtcNow,
                Status = LeadStatus.New,
                IpAddress = submission.IpAddress,
                UserAgent = submission.UserAgent,
                Referrer = submission.Referrer,
                UtmSource = submission.UtmSource,
                UtmMedium = submission.UtmMedium,
                UtmCampaign = submission.UtmCampaign
            };

            // Map form fields to lead fields
            foreach (var field in form.Fields.Where(f => !string.IsNullOrEmpty(f.LeadFieldMapping)))
            {
                if (formData.TryGetProperty(field.FieldName, out var fieldValue))
                {
                    var value = fieldValue.ValueKind == JsonValueKind.String 
                        ? fieldValue.GetString() 
                        : fieldValue.ToString();

                    switch (field.LeadFieldMapping?.ToLower())
                    {
                        case "email":
                            lead.Email = value ?? string.Empty;
                            break;
                        case "firstname":
                            lead.FirstName = value;
                            break;
                        case "lastname":
                            lead.LastName = value;
                            break;
                        case "phone":
                            lead.Phone = value;
                            break;
                        case "company":
                            lead.Company = value;
                            break;
                        case "jobtitle":
                            lead.JobTitle = value;
                            break;
                        case "website":
                            lead.Website = value;
                            break;
                        case "city":
                            lead.City = value;
                            break;
                        case "state":
                            lead.State = value;
                            break;
                        case "country":
                            lead.Country = value;
                            break;
                        case "industry":
                            lead.Industry = value;
                            break;
                    }
                }
            }

            // Set pipeline and stage
            if (form.PipelineId.HasValue)
            {
                lead.PipelineId = form.PipelineId;
                lead.StageId = form.DefaultStageId;
                lead.StageEnteredAt = DateTime.UtcNow;
            }

            // Store additional data as custom fields
            lead.CustomFields = formData.GetRawText();

            _context.Leads.Add(lead);
            await _context.SaveChangesAsync();

            submission.LeadId = lead.Id;

            // Create lead activity
            var activity = new LeadActivity
            {
                LeadId = lead.Id,
                TenantId = form.TenantId,
                Type = ActivityType.FormSubmitted,
                Title = $"Submitted form: {form.Name}",
                Metadata = JsonSerializer.Serialize(new { formId = form.Id, formName = form.Name }),
                CreatedAt = DateTime.UtcNow
            };
            _context.LeadActivities.Add(activity);
        }

        _context.FormSubmissions.Add(submission);

        // Update form statistics
        form.Submissions++;
        form.ConversionRate = form.Views > 0 ? (decimal)form.Submissions / form.Views * 100 : 0;

        // Update daily analytics
        var today = DateTime.UtcNow.Date;
        var analytics = await _context.FormAnalytics
            .FirstOrDefaultAsync(a => a.FormId == form.Id && a.Date == today);

        if (analytics == null)
        {
            analytics = new FormAnalytics
            {
                FormId = form.Id,
                Date = today
            };
            _context.FormAnalytics.Add(analytics);
        }

        analytics.Submissions++;
        analytics.ConversionRate = analytics.Views > 0 ? (decimal)analytics.Submissions / analytics.Views * 100 : 0;

        await _context.SaveChangesAsync();

        // Prepare response based on submission action
        var response = new
        {
            success = true,
            action = form.SubmissionAction.ToString().ToLower(),
            message = form.SuccessMessage,
            redirectUrl = form.RedirectUrl
        };

        return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message, details = ex.InnerException?.Message });
        }
    }

    // GET: api/FormApi/script
    [HttpGet("script")]
    public IActionResult GetScript()
    {
        var script = @"
(function() {
    'use strict';
    
    window.OptinYetiForms = window.OptinYetiForms || {};
    
    OptinYetiForms.init = function(config) {
        const embedCode = config.embedCode;
        const containerId = config.containerId || 'optinyeti-form-' + embedCode;
        const baseUrl = config.baseUrl || window.location.origin;
        
        // Create container if needed
        let container = document.getElementById(containerId);
        if (!container) {
            const scriptTag = document.currentScript || document.querySelector('script[data-oy-form]');
            if (scriptTag) {
                container = document.createElement('div');
                container.id = containerId;
                scriptTag.parentNode.insertBefore(container, scriptTag);
            }
        }
        
        if (!container) {
            console.error('OptinYeti Forms: Container not found');
            return;
        }
        
        // Load form
        fetch(baseUrl + '/api/FormApi/render/' + embedCode)
            .then(response => response.json())
            .then(form => {
                container.innerHTML = OptinYetiForms.renderForm(form, baseUrl, embedCode);
                OptinYetiForms.attachHandlers(container, form, baseUrl, embedCode);
            })
            .catch(error => {
                console.error('OptinYeti Forms: Error loading form', error);
                container.innerHTML = '<p style=""color: #ef4444; text-align: center;"">Form could not be loaded</p>';
            });
    };
    
    OptinYetiForms.renderForm = function(form, baseUrl, embedCode) {
        const s = form.settings;
        const c = form.content;
        
        let html = `
            <div class=""oy-form-wrapper"" style=""
                background-color: ${s.backgroundColor};
                color: ${s.textColor};
                border: 1px solid ${s.borderColor};
                border-radius: ${s.borderRadius}px;
                font-family: ${s.fontFamily};
                font-size: ${s.fontSize}px;
                padding: 24px;
                max-width: 500px;
                margin: 0 auto;
            "">
                ${c.headerText ? `<h2 style=""margin: 0 0 8px 0; font-size: 1.5em;"">${c.headerText}</h2>` : ''}
                ${c.subheaderText ? `<p style=""margin: 0 0 20px 0; opacity: 0.8;"">${c.subheaderText}</p>` : ''}
                <form class=""oy-form"" novalidate>
                    ${form.enableHoneypot ? '<input type=""text"" name=""_hp_field"" style=""display:none !important"" tabindex=""-1"" autocomplete=""off"">' : ''}
                    <input type=""hidden"" name=""_page_url"" value=""${window.location.href}"">
                    ${OptinYetiForms.renderFields(form.fields, s)}
                    <button type=""submit"" style=""
                        width: 100%;
                        padding: 12px 24px;
                        background-color: ${s.buttonColor};
                        color: ${s.buttonTextColor};
                        border: none;
                        border-radius: ${s.borderRadius}px;
                        font-size: 1em;
                        font-weight: 600;
                        cursor: pointer;
                        transition: opacity 0.2s;
                    "" onmouseover=""this.style.opacity='0.9'"" onmouseout=""this.style.opacity='1'"">${c.submitButtonText}</button>
                </form>
                ${c.footerText ? `<p style=""margin: 16px 0 0 0; font-size: 0.85em; opacity: 0.7; text-align: center;"">${c.footerText}</p>` : ''}
                ${c.showPoweredBy ? `<p style=""margin: 16px 0 0 0; font-size: 0.75em; opacity: 0.5; text-align: center;"">Powered by <a href=""https://optinyeti.com"" target=""_blank"" style=""color: inherit;"">OptinYeti</a></p>` : ''}
                <div class=""oy-form-message"" style=""display: none; padding: 12px; border-radius: ${s.borderRadius}px; margin-top: 16px; text-align: center;""></div>
            </div>
        `;
        
        if (s.customCss) {
            html += `<style>${s.customCss}</style>`;
        }
        
        return html;
    };
    
    OptinYetiForms.renderFields = function(fields, settings) {
        return fields.map(field => {
            let input = '';
            const inputStyle = `
                width: 100%;
                padding: 10px 12px;
                border: 1px solid ${settings.borderColor};
                border-radius: ${settings.borderRadius}px;
                font-size: 1em;
                font-family: inherit;
                box-sizing: border-box;
            `;
            
            switch(field.type) {
                case 'textarea':
                    input = `<textarea name=""${field.name}"" placeholder=""${field.placeholder || ''}"" ${field.required ? 'required' : ''} style=""${inputStyle} min-height: 100px; resize: vertical;""></textarea>`;
                    break;
                case 'select':
                    input = `<select name=""${field.name}"" ${field.required ? 'required' : ''} style=""${inputStyle}"">
                        <option value="""">${field.placeholder || 'Select...'}</option>
                        ${(field.options || []).map(opt => `<option value=""${opt}"">${opt}</option>`).join('')}
                    </select>`;
                    break;
                case 'checkbox':
                    input = `<label style=""display: flex; align-items: flex-start; gap: 8px; cursor: pointer;"">
                        <input type=""checkbox"" name=""${field.name}"" value=""1"" ${field.required ? 'required' : ''} style=""margin-top: 3px;"">
                        <span>${field.label}</span>
                    </label>`;
                    return `<div style=""margin-bottom: 16px; width: ${field.width}%;"">${input}${field.helpText ? `<small style=""display: block; margin-top: 4px; opacity: 0.7;"">${field.helpText}</small>` : ''}</div>`;
                case 'radio':
                    input = (field.options || []).map((opt, i) => `
                        <label style=""display: flex; align-items: center; gap: 8px; cursor: pointer; margin-bottom: 4px;"">
                            <input type=""radio"" name=""${field.name}"" value=""${opt}"" ${field.required && i === 0 ? 'required' : ''}>
                            <span>${opt}</span>
                        </label>
                    `).join('');
                    break;
                case 'consent':
                    input = `<label style=""display: flex; align-items: flex-start; gap: 8px; cursor: pointer;"">
                        <input type=""checkbox"" name=""${field.name}"" value=""1"" ${field.required ? 'required' : ''} style=""margin-top: 3px;"">
                        <span style=""font-size: 0.9em;"">${field.label}</span>
                    </label>`;
                    return `<div style=""margin-bottom: 16px; width: ${field.width}%;"">${input}</div>`;
                case 'heading':
                    return `<h3 style=""margin: 16px 0 8px 0;"">${field.label}</h3>`;
                case 'paragraph':
                    return `<p style=""margin: 0 0 16px 0; opacity: 0.8;"">${field.label}</p>`;
                case 'divider':
                    return `<hr style=""margin: 16px 0; border: none; border-top: 1px solid ${settings.borderColor};"">`;
                case 'hidden':
                    return `<input type=""hidden"" name=""${field.name}"" value=""${field.defaultValue || ''}"">`;
                default:
                    const inputType = field.type === 'email' ? 'email' : field.type === 'phone' ? 'tel' : field.type === 'number' ? 'number' : field.type === 'date' ? 'date' : 'text';
                    input = `<input type=""${inputType}"" name=""${field.name}"" placeholder=""${field.placeholder || ''}"" ${field.required ? 'required' : ''} ${field.minLength ? `minlength=""${field.minLength}""` : ''} ${field.maxLength ? `maxlength=""${field.maxLength}""` : ''} ${field.validationPattern ? `pattern=""${field.validationPattern}""` : ''} value=""${field.defaultValue || ''}"" style=""${inputStyle}"">`;
            }
            
            return `
                <div style=""margin-bottom: 16px; width: ${field.width}%;"">
                    ${field.type !== 'checkbox' && field.type !== 'consent' ? `<label style=""display: block; margin-bottom: 6px; font-weight: 500;"">${field.label}${field.required ? ' <span style=""color: #ef4444;"">*</span>' : ''}</label>` : ''}
                    ${input}
                    ${field.helpText && field.type !== 'checkbox' ? `<small style=""display: block; margin-top: 4px; opacity: 0.7;"">${field.helpText}</small>` : ''}
                </div>
            `;
        }).join('');
    };
    
    OptinYetiForms.attachHandlers = function(container, form, baseUrl, embedCode) {
        const formEl = container.querySelector('.oy-form');
        const messageEl = container.querySelector('.oy-form-message');
        const submitBtn = formEl.querySelector('button[type=""submit""]');
        
        formEl.addEventListener('submit', function(e) {
            e.preventDefault();
            
            // Disable button
            submitBtn.disabled = true;
            submitBtn.textContent = 'Submitting...';
            
            // Collect form data
            const formData = {};
            const inputs = formEl.querySelectorAll('input, textarea, select');
            inputs.forEach(input => {
                if (input.type === 'checkbox') {
                    formData[input.name] = input.checked ? '1' : '0';
                } else if (input.type === 'radio') {
                    if (input.checked) formData[input.name] = input.value;
                } else {
                    formData[input.name] = input.value;
                }
            });
            
            // Add UTM params from URL
            const urlParams = new URLSearchParams(window.location.search);
            ['utm_source', 'utm_medium', 'utm_campaign', 'utm_content', 'utm_term'].forEach(param => {
                if (urlParams.has(param)) formData['_' + param] = urlParams.get(param);
            });
            
            // Submit
            fetch(baseUrl + '/api/FormApi/submit/' + embedCode, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(formData)
            })
            .then(response => response.json())
            .then(result => {
                if (result.success) {
                    if (result.action === 'redirecttourl' && result.redirectUrl) {
                        window.location.href = result.redirectUrl;
                    } else {
                        formEl.style.display = 'none';
                        messageEl.style.display = 'block';
                        messageEl.style.backgroundColor = '#10b981';
                        messageEl.style.color = '#ffffff';
                        messageEl.textContent = result.message || 'Thank you for your submission!';
                        
                        if (result.action === 'showmessagethenredirect' && result.redirectUrl) {
                            setTimeout(() => { window.location.href = result.redirectUrl; }, 3000);
                        }
                    }
                } else {
                    throw new Error(result.error || 'Submission failed');
                }
            })
            .catch(error => {
                messageEl.style.display = 'block';
                messageEl.style.backgroundColor = '#ef4444';
                messageEl.style.color = '#ffffff';
                messageEl.textContent = 'Something went wrong. Please try again.';
                submitBtn.disabled = false;
                submitBtn.textContent = form.content.submitButtonText;
            });
        });
    };
    
    // Auto-initialize forms with data attributes
    document.addEventListener('DOMContentLoaded', function() {
        document.querySelectorAll('[data-oy-form]').forEach(el => {
            OptinYetiForms.init({
                embedCode: el.getAttribute('data-oy-form'),
                containerId: el.id,
                baseUrl: el.getAttribute('data-oy-base') || window.location.origin
            });
        });
    });
})();
";
        return Content(script, "application/javascript");
    }
}
