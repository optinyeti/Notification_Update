using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Notification_Application.Models;
using Notification_Application.Services;
using Notification_Application.Data;
using Microsoft.EntityFrameworkCore;

namespace Notification_Application.Controllers;

[Authorize]
public class PaymentController : Controller
{
    private readonly IStripeService _stripeService;
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;

    public PaymentController(IStripeService stripeService, UserManager<User> userManager, ApplicationDbContext context)
    {
        _stripeService = stripeService;
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Plans()
    {
        var plans = await _context.SubscriptionPlans
            .Where(p => p.IsActive)
            .OrderBy(p => p.MonthlyPrice)
            .ToListAsync();

        return View(plans);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCheckoutSession(int planId, string billingPeriod = "monthly")
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        try
        {
            var successUrl = Url.Action("Success", "Payment", null, Request.Scheme) ?? "";
            var cancelUrl = Url.Action("Plans", "Payment", null, Request.Scheme) ?? "";

            var session = await _stripeService.CreateCheckoutSessionAsync(
                user.TenantId,
                planId,
                billingPeriod,
                successUrl,
                cancelUrl
            );

            return Redirect(session.Url);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error creating checkout session: {ex.Message}";
            return RedirectToAction("Plans");
        }
    }

    [HttpGet]
    public IActionResult Success()
    {
        TempData["Success"] = "Subscription activated successfully! Welcome to your new plan.";
        return RedirectToAction("Subscription", "Admin");
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var stripeSignature = Request.Headers["Stripe-Signature"].ToString();

        try
        {
            await _stripeService.HandleWebhookEventAsync(json, stripeSignature);
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Webhook error: {ex.Message}");
            return BadRequest();
        }
    }

    [HttpPost]
    public async Task<IActionResult> ManageBilling()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var returnUrl = Url.Action("Subscription", "Admin", null, Request.Scheme) ?? "";
        
        try
        {
            var portalUrl = await _stripeService.CreateCustomerPortalSessionAsync(user.TenantId, returnUrl);
            
            if (string.IsNullOrEmpty(portalUrl))
            {
                TempData["Error"] = "No active subscription found";
                return RedirectToAction("Subscription", "Admin");
            }

            return Redirect(portalUrl);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error accessing billing portal: {ex.Message}";
            return RedirectToAction("Subscription", "Admin");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CancelSubscription()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        try
        {
            await _stripeService.CancelSubscriptionAsync(user.TenantId);
            TempData["Success"] = "Subscription canceled successfully";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error canceling subscription: {ex.Message}";
        }

        return RedirectToAction("Subscription", "Admin");
    }
}
