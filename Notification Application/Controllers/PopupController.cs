using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Notification_Application.Models;
using Notification_Application.Services;
using Notification_Application.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Notification_Application.Controllers;

[Authorize]
public class PopupController : Controller
{
    private readonly IPopupService _popupService;
    private readonly IPopupTemplateService _templateService;
    private readonly IAnalyticsService _analyticsService;
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;

    public PopupController(
        IPopupService popupService,
        IPopupTemplateService templateService,
        IAnalyticsService analyticsService,
        UserManager<User> userManager,
        ApplicationDbContext context)
    {
        _popupService = popupService;
        _templateService = templateService;
        _analyticsService = analyticsService;
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var popups = await _popupService.GetPopupsAsync(user.TenantId);
        // Compute public host for embed snippets
        var tenant = await _context.Tenants.FindAsync(user.TenantId);
        var publicHost = tenant?.PublicHostUrl
            ?? HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value;
        ViewBag.PublicHost = publicHost;

        return View(popups);
    }

    public IActionResult CreateCampaign()
    {
        return View();
    }

    public IActionResult Playbooks()
    {
        return View();
    }

    public async Task<IActionResult> Designer(int? id, string? name, PopupType? type)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        Popup popup;

        if (id.HasValue)
        {
            // Editing existing popup
            popup = await _popupService.GetPopupAsync(id.Value, user.TenantId);
            if (popup == null) return NotFound();
        }
        else
        {
            // Create a new popup with the provided name and type
            popup = new Popup
            {
                Name = name ?? "New Campaign",
                Type = type ?? PopupType.EmailCollector,
                TenantId = user.TenantId,
                CreatedById = user.Id,
                Status = PopupStatus.Draft,
                Content = "{}",
                TargetingRules = "{}",
                Title = "Your Campaign Title",
                Subtitle = "Add your subtitle here",
                CallToAction = "Get Started",
                ShowOnMobile = true,
                ShowOnDesktop = true
            };
            
            // Save the new popup first
            try
            {
                await _popupService.CreatePopupAsync(popup);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Popup limit exceeded"))
            {
                TempData["Error"] = "You've reached your popup limit for your current subscription plan. Please upgrade your plan or delete some existing popups.";
                return RedirectToAction("Index");
            }
        }

        return View(popup);
    }

    public IActionResult SelectType()
    {
        return View();
    }

    public async Task<IActionResult> SelectTemplate(PopupType? type = null)
    {
        var templates = type.HasValue 
            ? await _templateService.GetTemplatesByTypeAsync(type.Value)
            : await _templateService.GetAllTemplatesAsync();
        ViewBag.PopupType = type;
        return View(templates);
    }

    public IActionResult Create(PopupType? type, int? templateId)
    {
        var model = new PopupCreateViewModel();
        
        if (type.HasValue)
        {
            model.Type = type.Value;
        }
        
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> CreateFromTemplate(int templateId)
    {
        var template = await _templateService.GetTemplateAsync(templateId);
        if (template == null) return NotFound();

        var model = new PopupCreateViewModel
        {
            Name = template.Name + " - Copy",
            Type = template.Type,
            Title = template.Title,
            Subtitle = template.Subtitle,
            CallToAction = template.CallToAction,
            Content = template.Content,
            TargetingRules = template.DefaultTargetingRules,
            Trigger = template.DefaultTrigger,
            DelayMs = template.DefaultDelayMs,
            Frequency = template.DefaultFrequency,
            TypeSpecificOptions = template.TypeSpecificOptions
        };

        return View("Create", model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(PopupCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var popup = new Popup
        {
            Name = model.Name,
            Type = model.Type,
            Title = model.Title,
            Subtitle = model.Subtitle,
            CallToAction = model.CallToAction,
            Content = model.Content,
            TargetingRules = model.TargetingRules,
            Trigger = model.Trigger,
            DelayMs = model.DelayMs,
            Frequency = model.Frequency,
            ShowOnMobile = model.ShowOnMobile,
            ShowOnDesktop = model.ShowOnDesktop,
            TenantId = user.TenantId,
            CreatedById = user.Id
        };

        try
        {
            await _popupService.CreatePopupAsync(popup);
            TempData["Success"] = "Popup created successfully!";
            return RedirectToAction("Edit", new { id = popup.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var popup = await _popupService.GetPopupAsync(id, user.TenantId);
        if (popup == null) return NotFound();

        var model = new PopupEditViewModel
        {
            Id = popup.Id,
            TenantId = popup.TenantId,
            Name = popup.Name,
            Type = popup.Type,
            Title = popup.Title,
            Subtitle = popup.Subtitle,
            CallToAction = popup.CallToAction,
            Content = popup.Content,
            TargetingRules = popup.TargetingRules,
            Trigger = popup.Trigger,
            DelayMs = popup.DelayMs,
            Frequency = popup.Frequency,
            ShowOnMobile = popup.ShowOnMobile,
            ShowOnDesktop = popup.ShowOnDesktop,
            Status = popup.Status
        };

        // Compute public host for embed snippets
        var tenant = await _context.Tenants.FindAsync(user.TenantId);
        var publicHost = tenant?.PublicHostUrl
            ?? HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value;
        ViewBag.PublicHost = publicHost;

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(PopupEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var popup = await _popupService.GetPopupAsync(model.Id, user.TenantId);
        if (popup == null) return NotFound();

        popup.Name = model.Name;
        popup.Title = model.Title;
        popup.Subtitle = model.Subtitle;
        popup.CallToAction = model.CallToAction;
        popup.Content = model.Content;
        popup.TargetingRules = model.TargetingRules;
        popup.Trigger = model.Trigger;
        popup.DelayMs = model.DelayMs;
        popup.Frequency = model.Frequency;
        popup.ShowOnMobile = model.ShowOnMobile;
        popup.ShowOnDesktop = model.ShowOnDesktop;

        await _popupService.UpdatePopupAsync(popup);
        TempData["Success"] = "Popup updated successfully!";
        
        return RedirectToAction("Edit", new { id = popup.Id });
    }

    [HttpPost]
    public async Task<IActionResult> SaveDesign(int id, [FromBody] PopupDesignModel design)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var popup = await _popupService.GetPopupAsync(id, user.TenantId);
        if (popup == null) return NotFound();

        // Update design content
        popup.Content = design.Content ?? "{}";
        popup.TargetingRules = design.TargetingRules ?? "{}";
        
        // Update display rules if provided
        if (!string.IsNullOrEmpty(design.Trigger))
        {
            if (Enum.TryParse<PopupTrigger>(design.Trigger, out var trigger))
                popup.Trigger = trigger;
        }
        
        if (!string.IsNullOrEmpty(design.Frequency))
        {
            if (Enum.TryParse<PopupFrequency>(design.Frequency, out var frequency))
                popup.Frequency = frequency;
        }
        
        if (design.DelayMs.HasValue)
            popup.DelayMs = design.DelayMs.Value;
        
        if (design.ShowOnMobile.HasValue)
            popup.ShowOnMobile = design.ShowOnMobile.Value;
        
        if (design.ShowOnDesktop.HasValue)
            popup.ShowOnDesktop = design.ShowOnDesktop.Value;

        await _popupService.UpdatePopupAsync(popup);

        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> Publish(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        await _popupService.PublishPopupAsync(id, user.TenantId);
        TempData["Success"] = "Popup published successfully!";

        return RedirectToAction("Edit", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Unpublish(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        await _popupService.UnpublishPopupAsync(id, user.TenantId);
        TempData["Success"] = "Popup unpublished successfully!";

        return RedirectToAction("Edit", new { id });
    }

    public async Task<IActionResult> LivePreview(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var popup = await _popupService.GetPopupAsync(id, user.TenantId);
        if (popup == null) return NotFound();

        return View(popup);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        await _popupService.DeletePopupAsync(id, user.TenantId);
        TempData["Success"] = "Popup deleted successfully!";

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Analytics(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var popup = await _popupService.GetPopupAsync(id, user.TenantId);
        if (popup == null) return NotFound();

        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;
        
        // Get daily analytics data
        var dailyAnalytics = await _analyticsService.GetDailyPopupAnalyticsAsync(id, startDate, endDate);

        var model = new PopupAnalyticsViewModel
        {
            Popup = popup,
            Analytics = dailyAnalytics,
            StartDate = startDate,
            EndDate = endDate
        };

        return View(model);
    }

    [AllowAnonymous]
    public async Task<IActionResult> GetScript(int id)
    {
        var script = await _popupService.GeneratePopupScriptAsync(id);
        return Content(script, "application/javascript");
    }

    [AllowAnonymous]
    [HttpGet("Popup/GetTenantScript/{tenantId}")]
    public async Task<IActionResult> GetTenantScript(int tenantId)
    {
        var script = await _popupService.GenerateTenantScriptAsync(tenantId);
        return Content(script, "application/javascript");
    }

    [AllowAnonymous]
    [HttpGet("Popup/GetTenantPopups/{tenantId}")]
    public async Task<IActionResult> GetTenantPopups(int tenantId)
    {
        var popups = await _context.Popups
            .Where(p => p.TenantId == tenantId && p.Status == PopupStatus.Published)
            .Select(p => new
            {
                id = p.Id,
                name = p.Name,
                type = p.Type.ToString(),
                content = p.Content,
                trigger = p.Trigger.ToString(),
                delay = p.DelayMs,
                frequency = p.Frequency.ToString(),
                showOnMobile = p.ShowOnMobile,
                showOnDesktop = p.ShowOnDesktop,
                targetingRules = p.TargetingRules
            })
            .ToListAsync();

        return Json(popups);
    }

    [AllowAnonymous]
    [HttpGet("Popup/GetPopupData/{id}")]
    public async Task<IActionResult> GetPopupData(int id)
    {
        // Get popup without tenant restriction for public embed
        var popup = await _context.Popups.FindAsync(id);
        if (popup == null || popup.Status != PopupStatus.Published)
            return NotFound();

        var data = new
        {
            id = popup.Id,
            name = popup.Name,
            type = popup.Type.ToString(),
            content = popup.Content,
            trigger = popup.Trigger.ToString(),
            delay = popup.DelayMs,
            frequency = popup.Frequency.ToString(),
            showOnMobile = popup.ShowOnMobile,
            showOnDesktop = popup.ShowOnDesktop
        };

        return Json(data);
    }

    public async Task<IActionResult> Preview(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var popup = await _popupService.GetPopupAsync(id, user.TenantId);
        if (popup == null) return NotFound();

        return View(popup);
    }
}