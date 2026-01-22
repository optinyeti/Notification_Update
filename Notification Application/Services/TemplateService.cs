using Notification_Application.Models;

namespace Notification_Application.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<TemplateService> _logger;
        private readonly string _templatesBasePath;

        private static readonly List<TemplateCategory> _categories = new()
        {
            new TemplateCategory { Id = "popup", Label = "Popup", FolderName = "Popup" },
            new TemplateCategory { Id = "select-template", Label = "Select Template", FolderName = "SelectTemplate" },
            new TemplateCategory { Id = "floating-bar", Label = "Floating Bar", FolderName = "FloatingBar" },
            new TemplateCategory { Id = "fullscreen", Label = "Fullscreen", FolderName = "Fullscreen" },
            new TemplateCategory { Id = "inline", Label = "Inline", FolderName = "Inline" },
            new TemplateCategory { Id = "slide-in", Label = "Slide-in", FolderName = "SlideIn" },
            new TemplateCategory { Id = "gamified", Label = "Gamified", FolderName = "Gamified" }
        };

        public TemplateService(IWebHostEnvironment environment, ILogger<TemplateService> logger)
        {
            _environment = environment;
            _logger = logger;
            _templatesBasePath = Path.Combine(_environment.ContentRootPath, "Templates");
            
            // Ensure templates directory exists
            if (!Directory.Exists(_templatesBasePath))
            {
                Directory.CreateDirectory(_templatesBasePath);
            }
        }

        public IEnumerable<TemplateCategory> GetAllCategories()
        {
            return _categories;
        }

        public async Task<IEnumerable<TemplateInfo>> GetTemplatesByCategoryAsync(string category)
        {
            try
            {
                var categoryFolder = _categories.FirstOrDefault(c => c.Id == category)?.FolderName;
                if (string.IsNullOrEmpty(categoryFolder))
                {
                    _logger.LogWarning("Invalid category: {Category}", category);
                    return Enumerable.Empty<TemplateInfo>();
                }

                var categoryPath = Path.Combine(_templatesBasePath, categoryFolder);
                
                if (!Directory.Exists(categoryPath))
                {
                    Directory.CreateDirectory(categoryPath);
                    return Enumerable.Empty<TemplateInfo>();
                }

                var htmlFiles = Directory.GetFiles(categoryPath, "*.html");
                var templates = new List<TemplateInfo>();

                foreach (var file in htmlFiles)
                {
                    var fileInfo = new FileInfo(file);
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    
                    templates.Add(new TemplateInfo
                    {
                        Name = fileInfo.Name,
                        Title = FormatTitle(fileName),
                        Category = category,
                        FilePath = $"/Templates/{categoryFolder}/{fileInfo.Name}",
                        ThumbnailPath = $"/Templates/{categoryFolder}/{fileName}.png",
                        CreatedAt = fileInfo.CreationTime,
                        ModifiedAt = fileInfo.LastWriteTime
                    });
                }

                return templates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting templates for category: {Category}", category);
                return Enumerable.Empty<TemplateInfo>();
            }
        }

        public async Task<string> GetTemplateContentAsync(string category, string templateName)
        {
            try
            {
                var categoryFolder = _categories.FirstOrDefault(c => c.Id == category)?.FolderName;
                if (string.IsNullOrEmpty(categoryFolder))
                {
                    throw new ArgumentException($"Invalid category: {category}");
                }

                var filePath = Path.Combine(_templatesBasePath, categoryFolder, templateName);
                
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Template not found: {templateName}");
                }

                // Security check: ensure the file is within the templates directory
                var fullPath = Path.GetFullPath(filePath);
                var basePath = Path.GetFullPath(_templatesBasePath);
                
                if (!fullPath.StartsWith(basePath))
                {
                    throw new UnauthorizedAccessException("Access denied");
                }

                return await File.ReadAllTextAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading template: {Category}/{TemplateName}", category, templateName);
                throw;
            }
        }

        public async Task<bool> SaveTemplateAsync(string category, string templateName, string content)
        {
            try
            {
                var categoryFolder = _categories.FirstOrDefault(c => c.Id == category)?.FolderName;
                if (string.IsNullOrEmpty(categoryFolder))
                {
                    throw new ArgumentException($"Invalid category: {category}");
                }

                var categoryPath = Path.Combine(_templatesBasePath, categoryFolder);
                
                if (!Directory.Exists(categoryPath))
                {
                    Directory.CreateDirectory(categoryPath);
                }

                var filePath = Path.Combine(categoryPath, templateName);
                
                // Security check: ensure the file is within the templates directory
                var fullPath = Path.GetFullPath(filePath);
                var basePath = Path.GetFullPath(_templatesBasePath);
                
                if (!fullPath.StartsWith(basePath))
                {
                    throw new UnauthorizedAccessException("Access denied");
                }

                // Ensure it's an HTML file
                if (!templateName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("Only HTML files are allowed");
                }

                await File.WriteAllTextAsync(filePath, content);
                _logger.LogInformation("Template saved: {Category}/{TemplateName}", category, templateName);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving template: {Category}/{TemplateName}", category, templateName);
                return false;
            }
        }

        public async Task<bool> DeleteTemplateAsync(string category, string templateName)
        {
            try
            {
                var categoryFolder = _categories.FirstOrDefault(c => c.Id == category)?.FolderName;
                if (string.IsNullOrEmpty(categoryFolder))
                {
                    throw new ArgumentException($"Invalid category: {category}");
                }

                var filePath = Path.Combine(_templatesBasePath, categoryFolder, templateName);
                
                if (!File.Exists(filePath))
                {
                    return false;
                }

                // Security check
                var fullPath = Path.GetFullPath(filePath);
                var basePath = Path.GetFullPath(_templatesBasePath);
                
                if (!fullPath.StartsWith(basePath))
                {
                    throw new UnauthorizedAccessException("Access denied");
                }

                File.Delete(filePath);
                _logger.LogInformation("Template deleted: {Category}/{TemplateName}", category, templateName);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting template: {Category}/{TemplateName}", category, templateName);
                return false;
            }
        }

        private string FormatTitle(string fileName)
        {
            // Convert filename to title format (e.g., "my-template" -> "My Template")
            return string.Join(" ", fileName.Split('-', '_')
                .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
        }
    }
}
