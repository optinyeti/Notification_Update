using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Notification_Application.Models;
using System.Security.Claims;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Api.V2010.Account.AvailablePhoneNumberCountry;
using Twilio.Types;

namespace Notification_Application.Controllers
{
    [Authorize]
    public class NumbersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public NumbersController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            
            // Initialize Twilio
            var accountSid = _configuration["Twilio:AccountSid"];
            var authToken = _configuration["Twilio:AuthToken"];
            if (!string.IsNullOrEmpty(accountSid) && !string.IsNullOrEmpty(authToken))
            {
                TwilioClient.Init(accountSid, authToken);
            }
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        // GET: Numbers
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var phoneNumbers = await _context.PhoneNumbers
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PurchasedDate)
                .ToListAsync();

            return View(phoneNumbers);
        }

        // GET: Numbers/Search
        public IActionResult Search()
        {
            return View();
        }

        // POST: Numbers/SearchAvailable
        [HttpPost]
        public async Task<IActionResult> SearchAvailable(string areaCode, string? city, string? state, string type = "local")
        {
            try
            {
                var accountSid = _configuration["Twilio:AccountSid"];
                var authToken = _configuration["Twilio:AuthToken"];

                if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken))
                {
                    return Json(new { success = false, message = "Twilio configuration not found" });
                }

                TwilioClient.Init(accountSid, authToken);

                List<dynamic> availableNumbers = new List<dynamic>();

                if (type == "tollfree")
                {
                    var numbers = await TollFreeResource.ReadAsync(
                        pathCountryCode: "US",
                        limit: 20
                    );

                    availableNumbers = numbers.Select(n => new
                    {
                        phoneNumber = n.PhoneNumber.ToString(),
                        friendlyName = n.FriendlyName,
                        locality = n.Locality ?? "",
                        region = n.Region ?? "",
                        postalCode = n.PostalCode ?? "",
                        capabilities = new
                        {
                            voice = n.Capabilities.Voice,
                            sms = n.Capabilities.Sms
                        }
                    }).Cast<dynamic>().ToList();
                }
                else
                {
                    var numbers = await LocalResource.ReadAsync(
                        pathCountryCode: "US",
                        areaCode: !string.IsNullOrEmpty(areaCode) ? int.Parse(areaCode) : null,
                        inLocality: city,
                        inRegion: state,
                        limit: 20
                    );

                    availableNumbers = numbers.Select(n => new
                    {
                        phoneNumber = n.PhoneNumber.ToString(),
                        friendlyName = n.FriendlyName,
                        locality = n.Locality ?? "",
                        region = n.Region ?? "",
                        postalCode = n.PostalCode ?? "",
                        capabilities = new
                        {
                            voice = n.Capabilities.Voice,
                            sms = n.Capabilities.Sms
                        }
                    }).Cast<dynamic>().ToList();
                }

                return Json(new { success = true, numbers = availableNumbers });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Numbers/Order
        public IActionResult Order(string phoneNumber, string type = "local")
        {
            ViewBag.PhoneNumber = phoneNumber;
            ViewBag.Type = type;
            
            // Pricing
            var pricing = type == "tollfree" 
                ? new { monthly = 2.00m, perMinute = 0.02m }
                : new { monthly = 1.15m, perMinute = 0.013m };

            ViewBag.MonthlyFee = pricing.monthly;
            ViewBag.PerMinuteRate = pricing.perMinute;

            return View();
        }

        // POST: Numbers/Purchase
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Purchase(string phoneNumber, string friendlyName, string forwardToNumber, 
            string type, string? utmSource, string? utmMedium, string? utmCampaign, string? utmTerm, string? utmContent)
        {
            try
            {
                var userId = GetUserId();
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction(nameof(Search));
                }

                // Check if user has access (could implement plan restrictions)
                // For now, allowing all authenticated users

                // Purchase from Twilio
                var accountSid = _configuration["Twilio:AccountSid"];
                var authToken = _configuration["Twilio:AuthToken"];

                if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken))
                {
                    TempData["Error"] = "Twilio configuration not found";
                    return RedirectToAction(nameof(Search));
                }

                TwilioClient.Init(accountSid, authToken);

                var purchasedNumber = await IncomingPhoneNumberResource.CreateAsync(
                    phoneNumber: new Twilio.Types.PhoneNumber(phoneNumber),
                    voiceUrl: new Uri($"{_configuration["AppUrl"]}/api/twilio/voice"),
                    voiceMethod: Twilio.Http.HttpMethod.Post,
                    statusCallback: new Uri($"{_configuration["AppUrl"]}/api/twilio/status"),
                    statusCallbackMethod: Twilio.Http.HttpMethod.Post
                );

                // Save to database
                var phoneNumberType = type == "tollfree" ? PhoneNumberType.TollFree : PhoneNumberType.Local;
                var pricing = type == "tollfree"
                    ? new { monthly = 2.00m, perMinute = 0.02m }
                    : new { monthly = 1.15m, perMinute = 0.013m };

                var dbPhoneNumber = new Notification_Application.Models.PhoneNumber
                {
                    UserId = userId,
                    Number = phoneNumber,
                    FriendlyName = friendlyName,
                    TwilioSid = purchasedNumber.Sid,
                    Type = phoneNumberType,
                    ForwardToNumber = forwardToNumber,
                    IsActive = true,
                    PurchasedDate = DateTime.UtcNow,
                    MonthlyFee = pricing.monthly,
                    PerMinuteRate = pricing.perMinute,
                    UtmSource = utmSource,
                    UtmMedium = utmMedium,
                    UtmCampaign = utmCampaign,
                    UtmTerm = utmTerm,
                    UtmContent = utmContent
                };

                // Extract location info from phone number if available
                var phoneInfo = await IncomingPhoneNumberResource.FetchAsync(purchasedNumber.Sid);
                if (!string.IsNullOrEmpty(phoneInfo.FriendlyName))
                {
                    var parts = phoneInfo.FriendlyName.Split(',');
                    if (parts.Length > 0) dbPhoneNumber.City = parts[0].Trim();
                    if (parts.Length > 1) dbPhoneNumber.State = parts[1].Trim();
                }

                _context.PhoneNumbers.Add(dbPhoneNumber);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Phone number purchased successfully!";
                return RedirectToAction(nameof(Details), new { id = dbPhoneNumber.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error purchasing number: {ex.Message}";
                return RedirectToAction(nameof(Search));
            }
        }

        // GET: Numbers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetUserId();
            var phoneNumber = await _context.PhoneNumbers
                .Include(p => p.PhoneCalls)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (phoneNumber == null)
            {
                return NotFound();
            }

            return View(phoneNumber);
        }

        // GET: Numbers/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetUserId();
            var phoneNumber = await _context.PhoneNumbers
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (phoneNumber == null)
            {
                return NotFound();
            }

            return View(phoneNumber);
        }

        // POST: Numbers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string friendlyName, string forwardToNumber, 
            string? utmSource, string? utmMedium, string? utmCampaign, string? utmTerm, string? utmContent)
        {
            var userId = GetUserId();
            var phoneNumber = await _context.PhoneNumbers
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (phoneNumber == null)
            {
                return NotFound();
            }

            try
            {
                phoneNumber.FriendlyName = friendlyName;
                phoneNumber.ForwardToNumber = forwardToNumber;
                phoneNumber.UtmSource = utmSource;
                phoneNumber.UtmMedium = utmMedium;
                phoneNumber.UtmCampaign = utmCampaign;
                phoneNumber.UtmTerm = utmTerm;
                phoneNumber.UtmContent = utmContent;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Phone number updated successfully!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating number: {ex.Message}";
                return View(phoneNumber);
            }
        }

        // POST: Numbers/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = GetUserId();
            var phoneNumber = await _context.PhoneNumbers
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (phoneNumber == null)
            {
                return NotFound();
            }

            try
            {
                // Release from Twilio
                if (!string.IsNullOrEmpty(phoneNumber.TwilioSid))
                {
                    var accountSid = _configuration["Twilio:AccountSid"];
                    var authToken = _configuration["Twilio:AuthToken"];

                    if (!string.IsNullOrEmpty(accountSid) && !string.IsNullOrEmpty(authToken))
                    {
                        TwilioClient.Init(accountSid, authToken);
                        await IncomingPhoneNumberResource.DeleteAsync(phoneNumber.TwilioSid);
                    }
                }

                phoneNumber.IsActive = false;
                phoneNumber.CanceledDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Phone number canceled successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error canceling number: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // GET: Numbers/Analytics
        public async Task<IActionResult> Analytics()
        {
            var userId = GetUserId();
            var phoneNumbers = await _context.PhoneNumbers
                .Where(p => p.UserId == userId)
                .Include(p => p.PhoneCalls)
                .ToListAsync();

            return View(phoneNumbers);
        }
    }
}
