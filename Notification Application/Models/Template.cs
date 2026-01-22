namespace Notification_Application.Models
{
    public class TemplateInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ThumbnailPath { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }

    public class TemplateCategory
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string FolderName { get; set; } = string.Empty;
    }

    public class SaveTemplateRequest
    {
        public string Content { get; set; } = string.Empty;
    }
}
