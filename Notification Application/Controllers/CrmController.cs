using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Notification_Application.Models;
using System.Text.Json;

namespace Notification_Application.Controllers;

[Authorize(Roles = "User,Admin,SuperAdmin")]
public class CrmController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public CrmController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: CRM Dashboard
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Get summary statistics
        var totalLeads = await _context.Leads
            .Where(l => l.TenantId == user.TenantId)
            .CountAsync();

        var newLeadsToday = await _context.Leads
            .Where(l => l.TenantId == user.TenantId && l.CapturedAt.Date == DateTime.UtcNow.Date)
            .CountAsync();

        var qualifiedLeads = await _context.Leads
            .Where(l => l.TenantId == user.TenantId && l.Status == LeadStatus.Qualified)
            .CountAsync();

        var convertedLeads = await _context.Leads
            .Where(l => l.TenantId == user.TenantId && l.Status == LeadStatus.Converted)
            .CountAsync();

        var conversionRate = totalLeads > 0 ? (convertedLeads * 100.0 / totalLeads) : 0;

        // Get pipeline data
        var pipelines = await _context.Pipelines
            .Where(p => p.TenantId == user.TenantId && p.IsActive)
            .Include(p => p.Stages)
            .Include(p => p.Leads)
            .OrderBy(p => p.Order)
            .ToListAsync();

        // Get recent activities
        var recentActivities = await _context.LeadActivities
            .Where(a => a.TenantId == user.TenantId)
            .Include(a => a.Lead)
            .Include(a => a.PerformedBy)
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .ToListAsync();

        // Get tasks
        var upcomingTasks = await _context.CrmTasks
            .Where(t => t.TenantId == user.TenantId && t.Status != CrmTaskStatus.Completed)
            .Include(t => t.Lead)
            .OrderBy(t => t.DueDate)
            .Take(5)
            .ToListAsync();

        // Get deals summary
        var totalDealValue = await _context.Deals
            .Where(d => d.TenantId == user.TenantId && d.Status == DealStatus.Open)
            .SumAsync(d => d.Amount);

        var wonDealsValue = await _context.Deals
            .Where(d => d.TenantId == user.TenantId && d.Status == DealStatus.Won)
            .SumAsync(d => d.Amount);

        ViewBag.TotalLeads = totalLeads;
        ViewBag.NewLeadsToday = newLeadsToday;
        ViewBag.QualifiedLeads = qualifiedLeads;
        ViewBag.ConvertedLeads = convertedLeads;
        ViewBag.ConversionRate = conversionRate;
        ViewBag.Pipelines = pipelines;
        ViewBag.RecentActivities = recentActivities;
        ViewBag.UpcomingTasks = upcomingTasks;
        ViewBag.TotalDealValue = totalDealValue;
        ViewBag.WonDealsValue = wonDealsValue;

        return View();
    }

    // GET: CRM/GetLeadActivity
    [HttpGet]
    public async Task<IActionResult> GetLeadActivity(int leadId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var activities = await _context.LeadActivities
            .Where(a => a.LeadId == leadId && a.TenantId == user.TenantId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .Select(a => new
            {
                type = a.Type.ToString(),
                title = a.Title,
                createdAt = a.CreatedAt
            })
            .ToListAsync();

        return Json(activities);
    }

    // GET: CRM/LeadDetail/5
    public async Task<IActionResult> LeadDetail(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var lead = await _context.Leads
            .Include(l => l.Popup)
            .Include(l => l.Pipeline)
            .Include(l => l.Stage)
            .Include(l => l.Owner)
            .Include(l => l.Tags)
            .Include(l => l.Activities.OrderByDescending(a => a.CreatedAt))
                .ThenInclude(a => a.PerformedBy)
            .Include(l => l.StageHistory.OrderByDescending(h => h.MovedAt))
                .ThenInclude(h => h.FromStage)
            .Include(l => l.StageHistory)
                .ThenInclude(h => h.ToStage)
            .Include(l => l.ScoreHistory.OrderByDescending(s => s.CalculatedAt))
            .Include(l => l.Tasks.OrderByDescending(t => t.DueDate))
            .Include(l => l.Deals)
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == user.TenantId);

        if (lead == null)
        {
            return NotFound();
        }

        // Get available stages for pipeline
        if (lead.PipelineId.HasValue)
        {
            var stages = await _context.PipelineStages
                .Where(s => s.PipelineId == lead.PipelineId.Value && s.IsActive)
                .OrderBy(s => s.Order)
                .ToListAsync();
            ViewBag.AvailableStages = stages;
        }

        // Get available pipelines
        var pipelines = await _context.Pipelines
            .Where(p => p.TenantId == user.TenantId && p.IsActive)
            .ToListAsync();
        ViewBag.Pipelines = pipelines;

        // Get available tags
        var tags = await _context.LeadTags
            .Where(t => t.TenantId == user.TenantId)
            .ToListAsync();
        ViewBag.AvailableTags = tags;

        // Get team members for assignment
        var teamMembers = await _userManager.Users
            .Where(u => u.TenantId == user.TenantId)
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email })
            .ToListAsync();
        ViewBag.TeamMembers = teamMembers;

        return View(lead);
    }

    // POST: CRM/AddActivity
    [HttpPost]
    public async Task<IActionResult> AddActivity(int leadId, ActivityType type, string title, string? description, string? metadata)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == leadId && l.TenantId == user.TenantId);

        if (lead == null)
        {
            return NotFound();
        }

        var activity = new LeadActivity
        {
            LeadId = leadId,
            TenantId = user.TenantId,
            Type = type,
            Title = title,
            Description = description,
            Metadata = metadata,
            PerformedByUserId = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.LeadActivities.Add(activity);

        // Update lead's last contacted
        if (type == ActivityType.Email || type == ActivityType.Call || type == ActivityType.Meeting)
        {
            lead.LastContactedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(LeadDetail), new { id = leadId });
    }

    // POST: CRM/UpdateStage
    [HttpPost]
    public async Task<IActionResult> UpdateStage(int leadId, int stageId, string? notes)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var lead = await _context.Leads
            .Include(l => l.Stage)
            .FirstOrDefaultAsync(l => l.Id == leadId && l.TenantId == user.TenantId);

        if (lead == null)
        {
            return NotFound();
        }

        var newStage = await _context.PipelineStages
            .FirstOrDefaultAsync(s => s.Id == stageId);

        if (newStage == null)
        {
            return NotFound();
        }

        // Calculate days in previous stage
        int daysInPreviousStage = 0;
        if (lead.StageEnteredAt.HasValue)
        {
            daysInPreviousStage = (int)(DateTime.UtcNow - lead.StageEnteredAt.Value).TotalDays;
        }

        // Create stage history record
        var history = new LeadStageHistory
        {
            LeadId = leadId,
            FromStageId = lead.StageId,
            ToStageId = stageId,
            MovedAt = DateTime.UtcNow,
            MovedByUserId = user.Id,
            Notes = notes,
            DaysInPreviousStage = daysInPreviousStage
        };

        _context.LeadStageHistories.Add(history);

        // Update lead stage
        lead.StageId = stageId;
        lead.StageEnteredAt = DateTime.UtcNow;

        // Add activity
        var activity = new LeadActivity
        {
            LeadId = leadId,
            TenantId = user.TenantId,
            Type = ActivityType.StageChanged,
            Title = $"Stage changed to {newStage.Name}",
            Description = notes,
            PerformedByUserId = user.Id
        };

        _context.LeadActivities.Add(activity);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(LeadDetail), new { id = leadId });
    }

    // POST: CRM/UpdateScore
    [HttpPost]
    public async Task<IActionResult> UpdateScore(int leadId, int score, string? reason)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == leadId && l.TenantId == user.TenantId);

        if (lead == null)
        {
            return NotFound();
        }

        // Save score history
        var scoreRecord = new LeadScore
        {
            LeadId = leadId,
            Score = score,
            Reason = reason,
            CalculatedAt = DateTime.UtcNow
        };

        _context.LeadScores.Add(scoreRecord);

        // Update lead score
        lead.Score = score;
        lead.LastScoreUpdate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, score = score });
    }

    // POST: CRM/AssignOwner
    [HttpPost]
    public async Task<IActionResult> AssignOwner(int leadId, string ownerId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == leadId && l.TenantId == user.TenantId);

        if (lead == null)
        {
            return NotFound();
        }

        var owner = await _userManager.FindByIdAsync(ownerId);
        if (owner == null || owner.TenantId != user.TenantId)
        {
            return NotFound();
        }

        lead.OwnerId = ownerId;

        // Add activity
        var activity = new LeadActivity
        {
            LeadId = leadId,
            TenantId = user.TenantId,
            Type = ActivityType.Custom,
            Title = $"Lead assigned to {owner.FirstName} {owner.LastName}",
            PerformedByUserId = user.Id
        };

        _context.LeadActivities.Add(activity);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(LeadDetail), new { id = leadId });
    }

    // POST: CRM/AddTask
    [HttpPost]
    public async Task<IActionResult> AddTask(int leadId, string title, string? description, DateTime? dueDate, TaskPriority priority, string? assignedToUserId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == leadId && l.TenantId == user.TenantId);

        if (lead == null)
        {
            return NotFound();
        }

        var task = new CrmTask
        {
            TenantId = user.TenantId,
            LeadId = leadId,
            Title = title,
            Description = description,
            DueDate = dueDate,
            Priority = priority,
            AssignedToUserId = assignedToUserId,
            CreatedByUserId = user.Id
        };

        _context.CrmTasks.Add(task);

        // Add activity
        var activity = new LeadActivity
        {
            LeadId = leadId,
            TenantId = user.TenantId,
            Type = ActivityType.Task,
            Title = $"Task created: {title}",
            PerformedByUserId = user.Id
        };

        _context.LeadActivities.Add(activity);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(LeadDetail), new { id = leadId });
    }

    // POST: CRM/CreateDeal
    [HttpPost]
    public async Task<IActionResult> CreateDeal(int leadId, string name, decimal amount, string currency, decimal probability, DateTime? expectedCloseDate)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == leadId && l.TenantId == user.TenantId);

        if (lead == null)
        {
            return NotFound();
        }

        var deal = new Deal
        {
            TenantId = user.TenantId,
            LeadId = leadId,
            Name = name,
            Amount = amount,
            Currency = currency,
            Probability = probability,
            ExpectedCloseDate = expectedCloseDate,
            OwnerId = user.Id
        };

        _context.Deals.Add(deal);

        // Add activity
        var activity = new LeadActivity
        {
            LeadId = leadId,
            TenantId = user.TenantId,
            Type = ActivityType.Custom,
            Title = $"Deal created: {name} - {currency}{amount:N2}",
            PerformedByUserId = user.Id
        };

        _context.LeadActivities.Add(activity);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(LeadDetail), new { id = leadId });
    }

    // GET: CRM/Pipeline
    public async Task<IActionResult> Pipeline(int? pipelineId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var pipelines = await _context.Pipelines
            .Where(p => p.TenantId == user.TenantId && p.IsActive)
            .Include(p => p.Stages.OrderBy(s => s.Order))
            .OrderBy(p => p.Order)
            .ToListAsync();

        if (!pipelines.Any())
        {
            // Create default pipeline if none exists
            await CreateDefaultPipeline(user.TenantId);
            pipelines = await _context.Pipelines
                .Where(p => p.TenantId == user.TenantId && p.IsActive)
                .Include(p => p.Stages.OrderBy(s => s.Order))
                .OrderBy(p => p.Order)
                .ToListAsync();
        }

        var selectedPipeline = pipelineId.HasValue
            ? pipelines.FirstOrDefault(p => p.Id == pipelineId.Value)
            : pipelines.FirstOrDefault(p => p.IsDefault) ?? pipelines.First();

        // Auto-assign any unassigned leads to the default pipeline's "New Lead" stage
        if (selectedPipeline != null)
        {
            var newLeadStage = selectedPipeline.Stages?
                .FirstOrDefault(s => s.Name.Equals("New Lead", StringComparison.OrdinalIgnoreCase) && s.IsActive);

            if (newLeadStage != null)
            {
                var unassignedLeads = await _context.Leads
                    .Where(l => l.TenantId == user.TenantId && (l.PipelineId == null || l.StageId == null))
                    .ToListAsync();

                if (unassignedLeads.Any())
                {
                    foreach (var lead in unassignedLeads)
                    {
                        lead.PipelineId = selectedPipeline.Id;
                        lead.StageId = newLeadStage.Id;
                        lead.StageEnteredAt = DateTime.UtcNow;
                    }
                    await _context.SaveChangesAsync();
                }
            }

            // Load leads for each stage
            foreach (var stage in selectedPipeline.Stages)
            {
                var leadsInStage = await _context.Leads
                    .Where(l => l.StageId == stage.Id && l.TenantId == user.TenantId)
                    .Include(l => l.Owner)
                    .OrderByDescending(l => l.CapturedAt)
                    .ToListAsync();

                ViewData[$"LeadsInStage_{stage.Id}"] = leadsInStage;
            }
        }

        ViewBag.Pipelines = pipelines;
        ViewBag.SelectedPipeline = selectedPipeline;

        return View(selectedPipeline);
    }

    private async System.Threading.Tasks.Task CreateDefaultPipeline(int tenantId)
    {
        var pipeline = new Pipeline
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
    }

    // POST: CRM/AssignAllLeadsToPipeline - Assign all unassigned leads to default pipeline
    [HttpPost]
    public async Task<IActionResult> AssignAllLeadsToPipeline()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Get or create default pipeline
        var pipeline = await _context.Pipelines
            .Include(p => p.Stages)
            .FirstOrDefaultAsync(p => p.TenantId == user.TenantId && p.IsDefault && p.IsActive);

        if (pipeline == null)
        {
            await CreateDefaultPipeline(user.TenantId);
            pipeline = await _context.Pipelines
                .Include(p => p.Stages)
                .FirstOrDefaultAsync(p => p.TenantId == user.TenantId && p.IsDefault && p.IsActive);
        }

        if (pipeline == null)
        {
            TempData["Error"] = "Could not create or find default pipeline.";
            return RedirectToAction(nameof(Pipeline));
        }

        // Find the "New Lead" stage
        var newLeadStage = pipeline.Stages?
            .FirstOrDefault(s => s.Name.Equals("New Lead", StringComparison.OrdinalIgnoreCase) && s.IsActive);

        if (newLeadStage == null)
        {
            TempData["Error"] = "Could not find 'New Lead' stage in pipeline.";
            return RedirectToAction(nameof(Pipeline));
        }

        // Get all leads without a pipeline/stage assignment
        var unassignedLeads = await _context.Leads
            .Where(l => l.TenantId == user.TenantId && (l.PipelineId == null || l.StageId == null))
            .ToListAsync();

        if (!unassignedLeads.Any())
        {
            TempData["Info"] = "All leads are already assigned to the pipeline.";
            return RedirectToAction(nameof(Pipeline));
        }

        // Assign each lead to the pipeline
        foreach (var lead in unassignedLeads)
        {
            lead.PipelineId = pipeline.Id;
            lead.StageId = newLeadStage.Id;
            lead.StageEnteredAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = $"Successfully assigned {unassignedLeads.Count} lead(s) to the pipeline.";
        return RedirectToAction(nameof(Pipeline));
    }

    // GET: CRM/Analytics
    public async Task<IActionResult> Analytics()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Get date range (last 30 days)
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        // Leads over time
        var leadsOverTime = await _context.Leads
            .Where(l => l.TenantId == user.TenantId && l.CapturedAt >= startDate)
            .GroupBy(l => l.CapturedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync();

        // Conversion funnel
        var totalLeads = await _context.Leads.Where(l => l.TenantId == user.TenantId).CountAsync();
        var contactedLeads = await _context.Leads.Where(l => l.TenantId == user.TenantId && l.Status >= LeadStatus.Contacted).CountAsync();
        var qualifiedLeads = await _context.Leads.Where(l => l.TenantId == user.TenantId && l.Status >= LeadStatus.Qualified).CountAsync();
        var convertedLeads = await _context.Leads.Where(l => l.TenantId == user.TenantId && l.Status == LeadStatus.Converted).CountAsync();

        // Lead sources
        var leadSources = await _context.Leads
            .Where(l => l.TenantId == user.TenantId)
            .GroupBy(l => l.LeadSource ?? "Unknown")
            .Select(g => new { Source = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        // Average time in each stage
        var stageMetrics = await _context.LeadStageHistories
            .Where(h => h.Lead!.TenantId == user.TenantId)
            .Include(h => h.FromStage)
            .GroupBy(h => h.FromStage!.Name)
            .Select(g => new
            {
                Stage = g.Key,
                AverageDays = g.Average(h => h.DaysInPreviousStage)
            })
            .ToListAsync();

        ViewBag.LeadsOverTime = JsonSerializer.Serialize(leadsOverTime);
        ViewBag.TotalLeads = totalLeads;
        ViewBag.ContactedLeads = contactedLeads;
        ViewBag.QualifiedLeads = qualifiedLeads;
        ViewBag.ConvertedLeads = convertedLeads;
        ViewBag.LeadSources = JsonSerializer.Serialize(leadSources);
        ViewBag.StageMetrics = JsonSerializer.Serialize(stageMetrics);

        return View();
    }

    // GET: CRM/Deals
    public async Task<IActionResult> Deals(string? status, string? search, int page = 1, int pageSize = 20)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var query = _context.Deals
            .Include(d => d.Lead)
            .Include(d => d.Owner)
            .Where(d => d.TenantId == user.TenantId);

        // Filter by status
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<DealStatus>(status, out var dealStatus))
        {
            query = query.Where(d => d.Status == dealStatus);
        }

        // Search
        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(d => 
                d.Name.ToLower().Contains(search) ||
                (d.Lead != null && (d.Lead.FirstName + " " + d.Lead.LastName).ToLower().Contains(search)) ||
                (d.Lead != null && d.Lead.Company != null && d.Lead.Company.ToLower().Contains(search)));
        }

        var totalDeals = await query.CountAsync();
        var deals = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Summary stats
        ViewBag.TotalDeals = await _context.Deals.Where(d => d.TenantId == user.TenantId).CountAsync();
        ViewBag.OpenDeals = await _context.Deals.Where(d => d.TenantId == user.TenantId && d.Status == DealStatus.Open).CountAsync();
        ViewBag.WonDeals = await _context.Deals.Where(d => d.TenantId == user.TenantId && d.Status == DealStatus.Won).CountAsync();
        ViewBag.LostDeals = await _context.Deals.Where(d => d.TenantId == user.TenantId && d.Status == DealStatus.Lost).CountAsync();
        ViewBag.TotalValue = await _context.Deals.Where(d => d.TenantId == user.TenantId && d.Status == DealStatus.Open).SumAsync(d => d.Amount);
        ViewBag.WonValue = await _context.Deals.Where(d => d.TenantId == user.TenantId && d.Status == DealStatus.Won).SumAsync(d => d.Amount);

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalDeals / pageSize);
        ViewBag.SearchTerm = search;
        ViewBag.StatusFilter = status;

        // Get pipeline stages for deals
        var pipeline = await _context.Pipelines
            .Include(p => p.Stages.OrderBy(s => s.Order))
            .FirstOrDefaultAsync(p => p.TenantId == user.TenantId && p.IsDefault);
        ViewBag.PipelineStages = pipeline?.Stages?.ToList() ?? new List<PipelineStage>();

        return View(deals);
    }

    // POST: CRM/UpdateDealStatus
    [HttpPost]
    public async Task<IActionResult> UpdateDealStatus(int dealId, DealStatus status)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var deal = await _context.Deals
            .FirstOrDefaultAsync(d => d.Id == dealId && d.TenantId == user.TenantId);

        if (deal == null) return NotFound();

        deal.Status = status;
        deal.UpdatedAt = DateTime.UtcNow;

        if (status == DealStatus.Won || status == DealStatus.Lost)
        {
            deal.ActualCloseDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Deals));
    }

    // POST: CRM/UpdateDealStage - Deals don't have stages, so redirect to Deals page
    [HttpPost]
    public async Task<IActionResult> UpdateDealStage(int dealId, int stageId)
    {
        // Note: Deal model doesn't have StageId - this is a placeholder for future enhancement
        // For now, just return success
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success = true, message = "Deals don't have stages in current model" });
        }

        return RedirectToAction(nameof(Deals));
    }
}
