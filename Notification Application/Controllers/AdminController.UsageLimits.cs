using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Microsoft.AspNetCore.Identity;
using Notification_Application.Models;

namespace Notification_Application.Controllers
{
    public partial class AdminController
    {
        // Usage Limits & AI Settings
        [Authorize(Roles = "SuperAdmin,MasterAdmin")]
        public async Task<IActionResult> UsageLimits()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var tenant = await _context.Tenants
                .Include(t => t.SubscriptionPlan)
                .FirstOrDefaultAsync(t => t.Id == user.TenantId);

            if (tenant == null) return NotFound();

            var apiUsageLogs = await _context.ApiUsages
                .Where(a => a.TenantId == user.TenantId)
                .OrderByDescending(a => a.RequestDate)
                .Take(100)
                .ToListAsync();

            var totalPopups = await _context.Popups.CountAsync(p => p.TenantId == user.TenantId);
            var totalViews = await _context.Popups.Where(p => p.TenantId == user.TenantId).SumAsync(p => p.Views);
            var totalForms = await _context.WebsiteForms.CountAsync(f => f.TenantId == user.TenantId);

            ViewBag.Tenant = tenant;
            ViewBag.ApiUsageLogs = apiUsageLogs;
            ViewBag.TotalPopups = totalPopups;
            ViewBag.TotalViews = totalViews;
            ViewBag.TotalForms = totalForms;
            
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,MasterAdmin")]
        public async Task<IActionResult> UpdateUsageLimits(int tenantId, int maxAIRequests, int maxPopups, int maxForms)
        {
            var tenant = await _context.Tenants
                .Include(t => t.SubscriptionPlan)
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
            {
                return Json(new { success = false, message = "Tenant not found" });
            }

            // Update plan limits
            if (tenant.SubscriptionPlan != null)
            {
                tenant.SubscriptionPlan.MaxAIRequests = maxAIRequests;
                tenant.SubscriptionPlan.MaxPopups = maxPopups;
                tenant.SubscriptionPlan.MaxForms = maxForms;
                
                await _context.SaveChangesAsync();
                
                return Json(new { success = true, message = "Usage limits updated successfully" });
            }

            return Json(new { success = false, message = "No subscription plan found" });
        }
    }
}
