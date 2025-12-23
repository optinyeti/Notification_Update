using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Notification_Application.Models;

namespace Notification_Application.Services;

public class PopupTemplateService : IPopupTemplateService
{
    private readonly ApplicationDbContext _context;

    public PopupTemplateService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PopupTemplate?> GetTemplateAsync(int id)
    {
        return await _context.PopupTemplates
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
    }

    public async Task<IEnumerable<PopupTemplate>> GetAllTemplatesAsync()
    {
        return await _context.PopupTemplates
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<PopupTemplate>> GetTemplatesByTypeAsync(PopupType type)
    {
        return await _context.PopupTemplates
            .Where(t => t.Type == type && t.IsActive)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<PopupTemplate> CreateTemplateAsync(PopupTemplate template)
    {
        _context.PopupTemplates.Add(template);
        await _context.SaveChangesAsync();
        return template;
    }

    public async Task<PopupTemplate> UpdateTemplateAsync(PopupTemplate template)
    {
        _context.PopupTemplates.Update(template);
        await _context.SaveChangesAsync();
        return template;
    }

    public async Task DeleteTemplateAsync(int id)
    {
        var template = await _context.PopupTemplates.FindAsync(id);
        if (template != null)
        {
            template.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
}
