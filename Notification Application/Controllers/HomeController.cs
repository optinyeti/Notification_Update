using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Notification_Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Notification_Application.Services;

namespace Notification_Application.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IPopupService _popupService;
        private readonly IAnalyticsService _analyticsService;

        public HomeController(
            ILogger<HomeController> logger, 
            UserManager<User> userManager,
            IPopupService popupService,
            IAnalyticsService analyticsService)
        {
            _logger = logger;
            _userManager = userManager;
            _popupService = popupService;
            _analyticsService = analyticsService;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return View();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get dashboard data
            var popups = await _popupService.GetPopupsAsync(user.TenantId);
            var analytics = await _analyticsService.GetAnalyticsSummaryAsync(user.TenantId);

            var model = new DashboardViewModel
            {
                TotalPopups = popups.Count(),
                ActivePopups = popups.Count(p => p.Status == PopupStatus.Published),
                AnalyticsSummary = analytics,
                RecentPopups = popups.Take(5).ToList()
            };

            return View("Dashboard", model);
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
