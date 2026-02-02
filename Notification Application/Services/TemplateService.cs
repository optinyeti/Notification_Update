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
            new TemplateCategory { Id = "home-services", Label = "🏠 Home Services", FolderName = "HomeServices" },
            new TemplateCategory { Id = "ecommerce", Label = "🛒 E-commerce", FolderName = "Ecommerce" },
            new TemplateCategory { Id = "professional-services", Label = "💼 Professional Services", FolderName = "ProfessionalServices" },
            new TemplateCategory { Id = "healthcare", Label = "🏥 Healthcare", FolderName = "Healthcare" },
            new TemplateCategory { Id = "education", Label = "🎓 Education", FolderName = "Education" },
            new TemplateCategory { Id = "saas", Label = "💻 SaaS", FolderName = "SaaS" },
            new TemplateCategory { Id = "hospitality", Label = "🏨 Hospitality", FolderName = "Hospitality" },
            new TemplateCategory { Id = "automotive", Label = "🚗 Automotive", FolderName = "Automotive" },
            new TemplateCategory { Id = "nonprofit", Label = "❤️ Non-Profit", FolderName = "NonProfit" },
            new TemplateCategory { Id = "lead-capture", Label = "📋 Lead Capture", FolderName = "LeadCapture" },
            new TemplateCategory { Id = "popup", Label = "📱 Popup", FolderName = "Popup" },
            new TemplateCategory { Id = "floating-bar", Label = "📊 Floating Bar", FolderName = "FloatingBar" },
            new TemplateCategory { Id = "fullscreen", Label = "🖥️ Fullscreen", FolderName = "Fullscreen" },
            new TemplateCategory { Id = "inline", Label = "📄 Inline", FolderName = "Inline" },
            new TemplateCategory { Id = "slide-in", Label = "➡️ Slide-in", FolderName = "SlideIn" },
            new TemplateCategory { Id = "gamified", Label = "🎮 Gamified", FolderName = "Gamified" }
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

                // Get both .html and .cshtml files
                var htmlFiles = Directory.GetFiles(categoryPath, "*.html");
                var cshtmlFiles = Directory.GetFiles(categoryPath, "*.cshtml");
                var allFiles = htmlFiles.Concat(cshtmlFiles).ToArray();
                
                var templates = new List<TemplateInfo>();

                foreach (var file in allFiles)
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

                var content = await File.ReadAllTextAsync(filePath);
                
                // Strip Razor directives from .cshtml files
                if (templateName.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
                {
                    content = StripRazorDirectives(content);
                }

                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading template: {Category}/{TemplateName}", category, templateName);
                throw;
            }
        }
        
        private string StripRazorDirectives(string content)
        {
            if (string.IsNullOrEmpty(content)) return content;
            
            // Remove @{ Layout = null; } and similar Razor blocks at the start
            var patterns = new[]
            {
                @"@\{\s*Layout\s*=\s*null\s*;\s*\}\s*",  // @{ Layout = null; }
                @"@\{\s*\}\s*",  // Empty @{ }
                @"@model\s+[^\r\n]+\s*",  // @model directives
                @"@using\s+[^\r\n]+\s*",  // @using directives
                @"@inject\s+[^\r\n]+\s*"  // @inject directives
            };
            
            foreach (var pattern in patterns)
            {
                content = System.Text.RegularExpressions.Regex.Replace(content, pattern, "", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            
            return content.TrimStart();
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
