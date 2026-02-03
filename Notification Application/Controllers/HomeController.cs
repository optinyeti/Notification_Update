using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Notification_Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Notification_Application.Services;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;

namespace Notification_Application.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IPopupService _popupService;
        private readonly IAnalyticsService _analyticsService;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger, 
            UserManager<User> userManager,
            IPopupService popupService,
            IAnalyticsService analyticsService,
            ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _popupService = popupService;
            _analyticsService = analyticsService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Redirect authenticated users to the main UserDashboard
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Index", "UserDashboard");
            }

            // For anonymous users, show the landing page with blog posts
            var blogPosts = await _context.BlogPosts
                .Where(p => p.Status == BlogPostStatus.Published)
                .OrderByDescending(p => p.PublishedAt)
                .Take(3)
                .Include(p => p.Categories)
                .ToListAsync();
            
            ViewBag.BlogPosts = blogPosts;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Demo()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Contact(ContactFormModel model)
        {
            if (ModelState.IsValid)
            {
                // In a real implementation, this would send an email or create a support ticket
                TempData["Success"] = "Thank you for your message. We'll get back to you soon!";
                return RedirectToAction("Contact");
            }
            return View(model);
        }

        public IActionResult Pricing()
        {
            return View();
        }

        public IActionResult Features()
        {
            return View();
        }

        // Dynamic page route - replaces #anchor links with real pages
        [Route("page/{slug}")]
        public async Task<IActionResult> Page(string slug)
        {
            var page = await _context.WebsitePages
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished);

            if (page == null)
            {
                return NotFound();
            }

            return View(page);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
