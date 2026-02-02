using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Notification_Application.Models;
using Notification_Application.Services;
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
        private readonly IUnsplashService _unsplashService;

        public MediaController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            IWebHostEnvironment environment,
            IUnsplashService unsplashService)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _unsplashService = unsplashService;
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

        // Unsplash API endpoints
        [HttpGet]
        [Route("api/unsplash/search")]
        public async Task<IActionResult> SearchUnsplash(string query, int page = 1, int perPage = 20)
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                    return BadRequest(new { error = "Query is required" });

                var result = await _unsplashService.SearchPhotosAsync(query, page, perPage);
                return Json(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/unsplash/random")]
        public async Task<IActionResult> GetRandomUnsplash(string? query = null)
        {
            try
            {
                var photo = await _unsplashService.GetRandomPhotoAsync(query);
                if (photo == null)
                    return NotFound(new { error = "No photo found" });

                return Json(photo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/unsplash/collections")]
        public async Task<IActionResult> GetUnsplashCollections(int page = 1, int perPage = 10)
        {
            try
            {
                var collections = await _unsplashService.GetCollectionsAsync(page, perPage);
                return Json(collections);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/unsplash/collections/{collectionId}")]
        public async Task<IActionResult> GetCollectionPhotos(string collectionId, int page = 1, int perPage = 20)
        {
            try
            {
                var result = await _unsplashService.GetCollectionPhotosAsync(collectionId, page, perPage);
                return Json(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/unsplash/photo/{photoId}")]
        public async Task<IActionResult> GetUnsplashPhoto(string photoId)
        {
            try
            {
                var photo = await _unsplashService.GetPhotoAsync(photoId);
                if (photo == null)
                    return NotFound(new { error = "Photo not found" });

                return Json(photo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // Suggested search terms for popups
        [HttpGet]
        [Route("api/unsplash/suggestions")]
        public IActionResult GetSearchSuggestions()
        {
            var suggestions = new[]
            {
                new { Category = "Marketing", Terms = new[] { "business", "marketing", "success", "teamwork", "growth", "startup" } },
                new { Category = "E-commerce", Terms = new[] { "shopping", "product", "sale", "discount", "gift", "delivery" } },
                new { Category = "Newsletter", Terms = new[] { "email", "newsletter", "communication", "subscribe", "inbox" } },
                new { Category = "Lead Capture", Terms = new[] { "contact", "handshake", "meeting", "office", "professional" } },
                new { Category = "Seasonal", Terms = new[] { "christmas", "halloween", "summer", "spring", "autumn", "winter" } },
                new { Category = "Abstract", Terms = new[] { "gradient", "abstract", "pattern", "geometric", "minimal", "texture" } },
                new { Category = "Food", Terms = new[] { "food", "restaurant", "cooking", "cafe", "coffee", "dessert" } },
                new { Category = "Technology", Terms = new[] { "technology", "computer", "laptop", "coding", "digital", "innovation" } },
                new { Category = "Health", Terms = new[] { "health", "fitness", "wellness", "yoga", "meditation", "nature" } },
                new { Category = "Real Estate", Terms = new[] { "house", "interior", "architecture", "home", "apartment", "living room" } }
            };

            return Json(suggestions);
        }
    }
}
