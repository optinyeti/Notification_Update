using System;
using System.ComponentModel.DataAnnotations;

namespace Notification_Application.Models
{
    public class MediaLibraryImage
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string FileName { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; }
        
        [MaxLength(255)]
        public string? OriginalFileName { get; set; }
        
        public long FileSize { get; set; }
        
        [MaxLength(50)]
        public string? MimeType { get; set; }
        
        [MaxLength(100)]
        public string? Title { get; set; }
        
        [MaxLength(500)]
        public string? AltText { get; set; }
        
        public int Width { get; set; }
        
        public int Height { get; set; }
        
        public DateTime UploadedAt { get; set; }
        
        // Navigation property
        public virtual User? User { get; set; }
    }
}
