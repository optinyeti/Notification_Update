using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification_Application.Models;
using Notification_Application.Services;

namespace Notification_Application.Controllers
{
    [Authorize]
    public class TemplateController : Controller
    {
        private readonly ITemplateService _templateService;
        private readonly ILogger<TemplateController> _logger;

        public TemplateController(ITemplateService templateService, ILogger<TemplateController> logger)
        {
            _templateService = templateService;
            _logger = logger;
        }

        // GET: /Template/SelectTemplate
        [HttpGet]
        public async Task<IActionResult> SelectTemplate()
        {
            var categories = _templateService.GetAllCategories();
            
            // Pre-load all templates for all categories
            var allTemplates = new Dictionary<string, IEnumerable<TemplateInfo>>();
            foreach (var category in categories)
            {
                var templates = await _templateService.GetTemplatesByCategoryAsync(category.Id);
                allTemplates[category.Id] = templates;
            }
            
            ViewBag.AllTemplates = allTemplates;
            return View(categories);
        }

        // API: Get all templates by category
        [HttpGet]
        [Route("api/templates/{category}")]
        public async Task<IActionResult> GetTemplates(string category)
        {
            try
            {
                var templates = await _templateService.GetTemplatesByCategoryAsync(category);
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting templates for category: {Category}", category);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // API: Get specific template content
        [HttpGet]
        [Route("api/templates/{category}/{templateName}")]
        public async Task<IActionResult> GetTemplate(string category, string templateName)
        {
            try
            {
                var content = await _templateService.GetTemplateContentAsync(category, templateName);
                return Ok(new { content });
            }
            catch (FileNotFoundException)
            {
                return NotFound(new { error = "Template not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting template: {Category}/{TemplateName}", category, templateName);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // API: Save template
        [HttpPost]
        [Route("api/templates/{category}/{templateName}")]
        public async Task<IActionResult> SaveTemplate(
            string category, 
            string templateName, 
            [FromBody] SaveTemplateRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Content))
                {
                    return BadRequest(new { error = "Content is required" });
                }

                var success = await _templateService.SaveTemplateAsync(category, templateName, request.Content);
                
                if (success)
                {
                    return Ok(new { success = true, message = "Template saved successfully" });
                }
                else
                {
                    return StatusCode(500, new { success = false, error = "Failed to save template" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving template: {Category}/{TemplateName}", category, templateName);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        // API: Delete template
        [HttpDelete]
        [Route("api/templates/{category}/{templateName}")]
        public async Task<IActionResult> DeleteTemplate(string category, string templateName)
        {
            try
            {
                var success = await _templateService.DeleteTemplateAsync(category, templateName);
                
                if (success)
                {
                    return Ok(new { success = true, message = "Template deleted successfully" });
                }
                else
                {
                    return NotFound(new { success = false, error = "Template not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting template: {Category}/{TemplateName}", category, templateName);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}
