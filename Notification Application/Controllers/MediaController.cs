using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Notification_Application.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Notification_Application.Controllers
{
    [Authorize]
    public class MediaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _environment;

        public MediaController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> GetLibrary()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Json(new List<MediaLibraryImage>());

                var images = await _context.MediaLibraryImages
                    .Where(i => i.UserId == user.Id)
                    .OrderByDescending(i => i.UploadedAt)
                    .ToListAsync();

                return Json(images);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = "No file uploaded" });

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                    return BadRequest(new { error = "Invalid file type. Only JPEG, PNG, GIF, and WebP are allowed." });

                // Validate file size (5MB max)
                if (file.Length > 5 * 1024 * 1024)
                    return BadRequest(new { error = "File size must be less than 5MB" });

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized(new { error = "User not authenticated" });

                // Create user-specific directory
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "media", user.Id);
                Directory.CreateDirectory(uploadsPath);

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, uniqueFileName);

                // Get image dimensions
                int width = 0, height = 0;
                using (var image = await Image.LoadAsync(file.OpenReadStream()))
                {
                    width = image.Width;
                    height = image.Height;
                    
                    // Save optimized image
                    await image.SaveAsync(filePath);
                }

                // Save to database
                var mediaImage = new MediaLibraryImage
                {
                    UserId = user.Id,
                    FileName = uniqueFileName,
                    FilePath = $"/uploads/media/{user.Id}/{uniqueFileName}",
                    OriginalFileName = file.FileName,
                    FileSize = file.Length,
                    MimeType = file.ContentType,
                    Width = width,
                    Height = height,
                    UploadedAt = DateTime.UtcNow
                };

                _context.MediaLibraryImages.Add(mediaImage);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    image = mediaImage
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDetails(int id, string title, string altText)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized(new { error = "User not authenticated" });

                var image = await _context.MediaLibraryImages
                    .FirstOrDefaultAsync(i => i.Id == id && i.UserId == user.Id);

                if (image == null)
                    return NotFound();

                image.Title = title;
                image.AltText = altText;
                await _context.SaveChangesAsync();

                return Json(new { success = true, image });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized(new { error = "User not authenticated" });
                var image = await _context.MediaLibraryImages
                    .FirstOrDefaultAsync(i => i.Id == id && i.UserId == user.Id);

                if (image == null)
                    return NotFound();

                // Delete physical file
                var physicalPath = Path.Combine(_environment.WebRootPath, image.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }

                _context.MediaLibraryImages.Remove(image);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
