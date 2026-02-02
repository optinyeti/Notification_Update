using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Notification_Application.Models;

namespace Notification_Application.Services;

public interface IPipelineService
{
    Task AssignLeadToPipelineAsync(Lead lead, int tenantId);
    Task<Pipeline?> GetOrCreateDefaultPipelineAsync(int tenantId);
}

public class PipelineService : IPipelineService
{
    private readonly ApplicationDbContext _context;

    public PipelineService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AssignLeadToPipelineAsync(Lead lead, int tenantId)
    {
        // Get or create default pipeline
        var pipeline = await GetOrCreateDefaultPipelineAsync(tenantId);
        
        if (pipeline == null) return;

        // Find "New Lead" stage
        var newLeadStage = pipeline.Stages?.FirstOrDefault(s => 
            s.Name.Equals("New Lead", StringComparison.OrdinalIgnoreCase) && s.IsActive);

        if (newLeadStage != null)
        {
            lead.PipelineId = pipeline.Id;
            lead.StageId = newLeadStage.Id;
            lead.StageEnteredAt = DateTime.UtcNow;
        }
    }

    public async Task<Pipeline?> GetOrCreateDefaultPipelineAsync(int tenantId)
    {
        var pipeline = await _context.Pipelines
            .Include(p => p.Stages)
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.IsDefault && p.IsActive);

        if (pipeline == null)
        {
            // Create default pipeline
            pipeline = new Pipeline
            {
                TenantId = tenantId,
                Name = "Sales Pipeline",
                Description = "Default sales pipeline",
                IsDefault = true,
                IsActive = true,
                Order = 0
            };
            _context.Pipelines.Add(pipeline);
            await _context.SaveChangesAsync();

            // Create default stages
            var stages = new[]
            {
                new PipelineStage { PipelineId = pipeline.Id, Name = "New Lead", Color = "#94a3b8", Order = 0, IsActive = true },
                new PipelineStage { PipelineId = pipeline.Id, Name = "Contacted", Color = "#0ea5e9", Order = 1, IsActive = true },
                new PipelineStage { PipelineId = pipeline.Id, Name = "Qualified", Color = "#8b5cf6", Order = 2, IsActive = true },
                new PipelineStage { PipelineId = pipeline.Id, Name = "Proposal Sent", Color = "#f59e0b", Order = 3, IsActive = true },
                new PipelineStage { PipelineId = pipeline.Id, Name = "Negotiation", Color = "#ec4899", Order = 4, IsActive = true },
                new PipelineStage { PipelineId = pipeline.Id, Name = "Won", Color = "#10b981", Order = 5, IsActive = true, IsWonStage = true },
                new PipelineStage { PipelineId = pipeline.Id, Name = "Lost", Color = "#ef4444", Order = 6, IsActive = true, IsLostStage = true }
            };
            _context.PipelineStages.AddRange(stages);
            await _context.SaveChangesAsync();

            // Reload pipeline with stages
            pipeline = await _context.Pipelines
                .Include(p => p.Stages)
                .FirstOrDefaultAsync(p => p.Id == pipeline.Id);
        }

        return pipeline;
    }
}
