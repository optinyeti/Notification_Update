using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Notification_Application.Controllers
{
    [Authorize(Roles = "User")]
    public class IntegrationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
