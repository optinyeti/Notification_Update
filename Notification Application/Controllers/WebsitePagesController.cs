using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Notification_Application.Models;
using System.Security.Claims;

namespace Notification_Application.Controllers;

[Authorize(Roles = "Admin,SuperAdmin")]
public class WebsitePagesController : Controller
{
    private readonly ApplicationDbContext _context;

    public WebsitePagesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Pages
    public async Task<IActionResult> Index()
    {
        var tenantId = User.FindFirstValue("TenantId");
        if (string.IsNullOrEmpty(tenantId) || !int.TryParse(tenantId, out int parsedTenantId))
        {
            return Unauthorized();
        }

        var pages = await _context.WebsitePages
            .Where(p => p.TenantId == parsedTenantId)
            .OrderByDescending(p => p.IsHomepage)
            .ThenBy(p => p.Title)
            .ToListAsync();

        return View(pages);
    }

    // GET: Pages/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Pages/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WebsitePage page)
    {
        var tenantId = User.FindFirstValue("TenantId");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(tenantId) || !int.TryParse(tenantId, out int parsedTenantId))
        {
            return Unauthorized();
        }

        if (ModelState.IsValid)
        {
            page.TenantId = parsedTenantId;
            page.CreatedById = userId;
            page.CreatedAt = DateTime.UtcNow;
            page.Slug = GenerateSlug(page.Title);

            // If this is set as homepage, unset other homepages
            if (page.IsHomepage)
            {
                var existingHomepage = await _context.WebsitePages
                    .FirstOrDefaultAsync(p => p.TenantId == parsedTenantId && p.IsHomepage);
                if (existingHomepage != null)
                {
                    existingHomepage.IsHomepage = false;
                }
            }

            if (page.IsPublished && page.PublishedAt == null)
            {
                page.PublishedAt = DateTime.UtcNow;
            }

            _context.Add(page);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Page created successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(page);
    }

    // GET: Pages/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var tenantId = User.FindFirstValue("TenantId");
        if (string.IsNullOrEmpty(tenantId) || !int.TryParse(tenantId, out int parsedTenantId))
        {
            return Unauthorized();
        }

        var page = await _context.WebsitePages
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == parsedTenantId);

        if (page == null) return NotFound();

        return View(page);
    }

    // POST: Pages/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, WebsitePage page)
    {
        if (id != page.Id) return NotFound();

        var tenantId = User.FindFirstValue("TenantId");
        if (string.IsNullOrEmpty(tenantId) || !int.TryParse(tenantId, out int parsedTenantId))
        {
            return Unauthorized();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existingPage = await _context.WebsitePages.FindAsync(id);
                if (existingPage == null || existingPage.TenantId != parsedTenantId)
                {
                    return NotFound();
                }

                existingPage.Title = page.Title;
                existingPage.Content = page.Content;
                existingPage.MetaDescription = page.MetaDescription;
                existingPage.MetaKeywords = page.MetaKeywords;
                existingPage.IsPublished = page.IsPublished;
                existingPage.UpdatedAt = DateTime.UtcNow;

                // If this is set as homepage, unset other homepages
                if (page.IsHomepage && !existingPage.IsHomepage)
                {
                    var otherHomepage = await _context.WebsitePages
                        .FirstOrDefaultAsync(p => p.TenantId == parsedTenantId && p.IsHomepage && p.Id != id);
                    if (otherHomepage != null)
                    {
                        otherHomepage.IsHomepage = false;
                    }
                }
                existingPage.IsHomepage = page.IsHomepage;

                if (page.IsPublished && existingPage.PublishedAt == null)
                {
                    existingPage.PublishedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Page updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PageExists(page.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(page);
    }

    // POST: Pages/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var tenantId = User.FindFirstValue("TenantId");
        if (string.IsNullOrEmpty(tenantId) || !int.TryParse(tenantId, out int parsedTenantId))
        {
            return Unauthorized();
        }

        var page = await _context.WebsitePages
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == parsedTenantId);

        if (page != null)
        {
            _context.WebsitePages.Remove(page);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Page deleted successfully!";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool PageExists(int id)
    {
        return _context.WebsitePages.Any(e => e.Id == id);
    }

    private string GenerateSlug(string title)
    {
        return title.ToLower()
            .Replace(" ", "-")
            .Replace("&", "and")
            .Replace(",", "")
            .Replace(".", "")
            .Replace("'", "")
            .Replace("\"", "");
    }
}
