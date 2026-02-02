using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification_Application.Data;
using Notification_Application.Models;
using System.Text.Json;

namespace Notification_Application.Controllers;

[Authorize(Roles = "User,Admin,SuperAdmin")]
public class LeadsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public LeadsController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Leads
    public async Task<IActionResult> Index(int? popupId, string? status, string? searchTerm, int page = 1)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Start with base query - no Include to avoid INNER JOIN issues
        var baseQuery = _context.Leads
            .Where(l => l.TenantId == user.TenantId)
            .AsQueryable();

        // Filter by popup
        if (popupId.HasValue)
        {
            baseQuery = baseQuery.Where(l => l.PopupId == popupId.Value);
        }

        // Filter by status
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<LeadStatus>(status, out var leadStatus))
        {
            baseQuery = baseQuery.Where(l => l.Status == leadStatus);
        }

        // Search
        if (!string.IsNullOrEmpty(searchTerm))
        {
            baseQuery = baseQuery.Where(l => 
                (l.Email != null && l.Email.Contains(searchTerm)) ||
                (l.FirstName != null && l.FirstName.Contains(searchTerm)) ||
                (l.LastName != null && l.LastName.Contains(searchTerm)) ||
                (l.Company != null && l.Company.Contains(searchTerm)) ||
                (l.Phone != null && l.Phone.Contains(searchTerm))
            );
        }

        // Get popups for filter dropdown
        var popups = await _context.Popups
            .Where(p => p.TenantId == user.TenantId)
            .Select(p => new { p.Id, p.Name })
            .ToListAsync();

        ViewBag.Popups = popups;
        ViewBag.CurrentPopupId = popupId;
        ViewBag.CurrentStatus = status;
        ViewBag.SearchTerm = searchTerm;
        ViewBag.CurrentPage = page;

        // Pagination
        int pageSize = 50;
        var totalLeads = await baseQuery.CountAsync();
        
        // Get leads with optional Popup (LEFT JOIN)
        var leads = await baseQuery
            .OrderByDescending(l => l.CapturedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        // Manually load popups for leads that have a valid PopupId
        var popupIds = leads.Where(l => l.PopupId.HasValue && l.PopupId > 0).Select(l => l.PopupId!.Value).Distinct().ToList();
        var popupDict = await _context.Popups
            .Where(p => popupIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);
        
        foreach (var lead in leads)
        {
            if (lead.PopupId.HasValue && lead.PopupId > 0 && popupDict.TryGetValue(lead.PopupId.Value, out var popup))
            {
                lead.Popup = popup;
            }
        }
        
        // Load tasks for leads
        var leadIds = leads.Select(l => l.Id).ToList();
        var tasks = await _context.CrmTasks
            .Where(t => t.LeadId.HasValue && leadIds.Contains(t.LeadId.Value))
            .ToListAsync();
        
        foreach (var lead in leads)
        {
            lead.Tasks = tasks.Where(t => t.LeadId == lead.Id).ToList();
        }

        ViewBag.TotalPages = (int)Math.Ceiling(totalLeads / (double)pageSize);
        ViewBag.TotalLeads = totalLeads;

        return View(leads);
    }

    // GET: Leads/GetLeadsJson - AJAX endpoint for getting leads
    [HttpGet]
    public async Task<IActionResult> GetLeadsJson()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var leads = await _context.Leads
            .Where(l => l.TenantId == user.TenantId)
            .OrderByDescending(l => l.CapturedAt)
            .Take(100)
            .Select(l => new
            {
                id = l.Id,
                firstName = l.FirstName,
                lastName = l.LastName,
                email = l.Email,
                company = l.Company
            })
            .ToListAsync();

        return Json(leads);
    }

    // GET: Leads/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var lead = await _context.Leads
            .Include(l => l.Popup)
            .Include(l => l.Tenant)
            .Include(l => l.Pipeline)
            .Include(l => l.Stage)
            .Include(l => l.AssignedDeal)
            .Include(l => l.Activities.OrderByDescending(a => a.CreatedAt).Take(10))
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == user.TenantId);

        if (lead == null)
        {
            return NotFound();
        }

        // Parse custom fields
        try
        {
            var customFields = JsonSerializer.Deserialize<Dictionary<string, object>>(lead.CustomFields);
            ViewBag.CustomFields = customFields;
        }
        catch
        {
            ViewBag.CustomFields = new Dictionary<string, object>();
        }

        // Get pipelines for dropdown
        ViewBag.Pipelines = await _context.Pipelines
            .Where(p => p.TenantId == user.TenantId && p.IsActive)
            .Include(p => p.Stages.OrderBy(s => s.Order))
            .ToListAsync();

        // Get deals for dropdown
        ViewBag.Deals = await _context.Deals
            .Where(d => d.TenantId == user.TenantId)
            .OrderByDescending(d => d.CreatedAt)
            .Take(50)
            .ToListAsync();

        // Check if user has CRM access (Pro plan or above)
        var tenant = await _context.Tenants
            .Include(t => t.SubscriptionPlan)
            .FirstOrDefaultAsync(t => t.Id == user.TenantId);
        ViewBag.HasCrmAccess = tenant?.SubscriptionPlanId >= 3;
        ViewBag.PlanId = tenant?.SubscriptionPlanId ?? 1;

        return View(lead);
    }

    // POST: Leads/UpdateStatus
    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, LeadStatus status, string? notes)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == user.TenantId);

        if (lead == null)
        {
            return NotFound();
        }

        var oldStatus = lead.Status;
        lead.Status = status;
        if (!string.IsNullOrEmpty(notes))
        {
            lead.Notes = notes;
        }
        
        if (status == LeadStatus.Contacted)
        {
            lead.LastContactedAt = DateTime.UtcNow;
        }

        // Auto-assign to pipeline stage based on status
        await AssignLeadToPipelineStage(lead, status, user.TenantId);

        // Log activity
        _context.LeadActivities.Add(new LeadActivity
        {
            LeadId = lead.Id,
            TenantId = user.TenantId,
            Type = ActivityType.StatusChanged,
            Title = $"Status changed from {oldStatus} to {status}",
            Description = notes,
            PerformedByUserId = user.Id,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Lead status updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // Helper method to assign lead to appropriate pipeline stage based on status
    private async System.Threading.Tasks.Task AssignLeadToPipelineStage(Lead lead, LeadStatus status, int tenantId)
    {
        // Get or create default pipeline
        var pipeline = await _context.Pipelines
            .Include(p => p.Stages)
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.IsDefault && p.IsActive);

        if (pipeline == null)
        {
            // Create default pipeline if none exists
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

        if (pipeline == null) return;

        // Map status to stage name
        string stageName = status switch
        {
            LeadStatus.New => "New Lead",
            LeadStatus.Contacted => "Contacted",
            LeadStatus.Qualified => "Qualified",
            LeadStatus.Converted => "Won",
            LeadStatus.Unqualified => "Lost",
            LeadStatus.Archived => "Lost",
            _ => "New Lead"
        };

        var stage = pipeline.Stages.FirstOrDefault(s => s.Name.Equals(stageName, StringComparison.OrdinalIgnoreCase) && s.IsActive);
        
        if (stage != null)
        {
            lead.PipelineId = pipeline.Id;
            lead.StageId = stage.Id;
            lead.StageEnteredAt = DateTime.UtcNow;
        }
    }

    // POST: Leads/UpdateDisposition
    [HttpPost]
    public async Task<IActionResult> UpdateDisposition(int id, LeadDisposition disposition, string? notes)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == user.TenantId);

        if (lead == null)
        {
            return NotFound();
        }

        var oldDisposition = lead.Disposition;
        lead.Disposition = disposition;
        
        if (!string.IsNullOrEmpty(notes))
        {
            lead.Notes = (lead.Notes ?? "") + "\n[" + DateTime.UtcNow.ToString("g") + "] " + notes;
        }

        // Log activity
        _context.LeadActivities.Add(new LeadActivity
        {
            LeadId = lead.Id,
            TenantId = user.TenantId,
            Type = ActivityType.Note,
            Title = $"Disposition changed from {oldDisposition} to {disposition}",
            Description = notes,
            PerformedByUserId = user.Id,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Lead disposition updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: Leads/AssignToPipeline
    [HttpPost]
    public async Task<IActionResult> AssignToPipeline(int id, int pipelineId, int? stageId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Check CRM access
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == user.TenantId);
        if (tenant?.SubscriptionPlanId < 3)
        {
            TempData["ErrorMessage"] = "Pipeline assignment requires a Pro plan or above.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == user.TenantId);

        if (lead == null)
        {
            return NotFound();
        }

        var pipeline = await _context.Pipelines
            .Include(p => p.Stages.OrderBy(s => s.Order))
            .FirstOrDefaultAsync(p => p.Id == pipelineId && p.TenantId == user.TenantId);

        if (pipeline == null)
        {
            return NotFound("Pipeline not found");
        }

        lead.PipelineId = pipelineId;
        lead.Pipeline = pipeline;

        // If no stage specified, use the first stage
        if (stageId.HasValue)
        {
            lead.StageId = stageId.Value;
        }
        else if (pipeline.Stages.Any())
        {
            lead.StageId = pipeline.Stages.First().Id;
        }

        lead.StageEnteredAt = DateTime.UtcNow;

        // Log activity
        _context.LeadActivities.Add(new LeadActivity
        {
            LeadId = lead.Id,
            TenantId = user.TenantId,
            Type = ActivityType.StageChanged,
            Title = $"Assigned to pipeline: {pipeline.Name}",
            PerformedByUserId = user.Id,
            CreatedAt = DateTime.UtcNow
        });

        // Add stage history
        if (lead.StageId.HasValue)
        {
            _context.LeadStageHistories.Add(new LeadStageHistory
            {
                LeadId = lead.Id,
                ToStageId = lead.StageId.Value,
                MovedAt = DateTime.UtcNow,
                MovedByUserId = user.Id
            });
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Lead assigned to pipeline successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: Leads/UpdateStage
    [HttpPost]
    public async Task<IActionResult> UpdateStage(int id, int stageId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Check CRM access
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == user.TenantId);
        if (tenant?.SubscriptionPlanId < 3)
        {
            TempData["ErrorMessage"] = "Stage management requires a Pro plan or above.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var lead = await _context.Leads
            .Include(l => l.Stage)
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == user.TenantId);

        if (lead == null)
        {
            return NotFound();
        }

        var newStage = await _context.PipelineStages
            .FirstOrDefaultAsync(s => s.Id == stageId);

        if (newStage == null)
        {
            return NotFound("Stage not found");
        }

        var oldStage = lead.Stage?.Name ?? "None";
        var oldStageId = lead.StageId;

        lead.StageId = stageId;
        lead.StageEnteredAt = DateTime.UtcNow;

        // Log activity
        _context.LeadActivities.Add(new LeadActivity
        {
            LeadId = lead.Id,
            TenantId = user.TenantId,
            Type = ActivityType.StageChanged,
            Title = $"Stage changed from {oldStage} to {newStage.Name}",
            PerformedByUserId = user.Id,
            CreatedAt = DateTime.UtcNow
        });

        // Add new stage history
        _context.LeadStageHistories.Add(new LeadStageHistory
        {
            LeadId = lead.Id,
            FromStageId = oldStageId,
            ToStageId = stageId,
            MovedAt = DateTime.UtcNow,
            MovedByUserId = user.Id
        });

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Lead stage updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: Leads/AssignDeal
    [HttpPost]
    public async Task<IActionResult> AssignDeal(int id, int? dealId, decimal? potentialValue, string? dealTitle)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Check CRM access
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == user.TenantId);
        if (tenant?.SubscriptionPlanId < 3)
        {
            TempData["ErrorMessage"] = "Deal assignment requires a Pro plan or above.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == user.TenantId);

        if (lead == null)
        {
            return NotFound();
        }

        lead.PotentialValue = potentialValue;

        if (dealId.HasValue && dealId.Value > 0)
        {
            // Assign existing deal
            var deal = await _context.Deals
                .FirstOrDefaultAsync(d => d.Id == dealId.Value && d.TenantId == user.TenantId);
            
            if (deal != null)
            {
                lead.AssignedDealId = deal.Id;
                
                // Log activity
                _context.LeadActivities.Add(new LeadActivity
                {
                    LeadId = lead.Id,
                    TenantId = user.TenantId,
                    Type = ActivityType.DealCreated,
                    Title = $"Assigned to deal: {deal.Name}",
                    Description = $"Deal value: {deal.Amount:C}",
                    PerformedByUserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        else if (!string.IsNullOrEmpty(dealTitle))
        {
            // Create new deal
            var newDeal = new Deal
            {
                Name = dealTitle,
                Amount = potentialValue ?? 0,
                TenantId = user.TenantId,
                LeadId = lead.Id,
                OwnerId = user.Id,
                Status = DealStatus.Open,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Deals.Add(newDeal);
            await _context.SaveChangesAsync();
            
            lead.AssignedDealId = newDeal.Id;

            // Log activity
            _context.LeadActivities.Add(new LeadActivity
            {
                LeadId = lead.Id,
                TenantId = user.TenantId,
                Type = ActivityType.DealCreated,
                Title = $"New deal created: {dealTitle}",
                Description = $"Deal value: {potentialValue:C}",
                PerformedByUserId = user.Id,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Deal assignment updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: Leads/AddNote
    [HttpPost]
    public async Task<IActionResult> AddNote(int id, string note)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == user.TenantId);

        if (lead == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(note))
        {
            lead.Notes = (lead.Notes ?? "") + "\n[" + DateTime.UtcNow.ToString("g") + "] " + note;

            // Log activity
            _context.LeadActivities.Add(new LeadActivity
            {
                LeadId = lead.Id,
                TenantId = user.TenantId,
                Type = ActivityType.Note,
                Title = "Note added",
                Description = note,
                PerformedByUserId = user.Id,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        TempData["SuccessMessage"] = "Note added successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: Leads/CreateTask
    [HttpPost]
    public async Task<IActionResult> CreateTask(int leadId, string title, string? description, DateTime dueDate, string priority, string? taskType)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == leadId && l.TenantId == user.TenantId);

        if (lead == null)
        {
            TempData["ErrorMessage"] = "Lead not found.";
            return RedirectToAction(nameof(Index));
        }

        // Parse priority
        if (!Enum.TryParse<TaskPriority>(priority, out var taskPriority))
        {
            taskPriority = TaskPriority.Medium;
        }

        var task = new CrmTask
        {
            TenantId = user.TenantId,
            LeadId = leadId,
            Title = title,
            Description = description + (taskType != null ? $"\n[Type: {taskType}]" : ""),
            DueDate = dueDate,
            Priority = taskPriority,
            Status = CrmTaskStatus.Pending,
            CreatedByUserId = user.Id,
            AssignedToUserId = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.CrmTasks.Add(task);

        // Log activity
        _context.LeadActivities.Add(new LeadActivity
        {
            LeadId = leadId,
            TenantId = user.TenantId,
            Type = ActivityType.Task,
            Title = $"Task created: {title}",
            Description = $"Due: {dueDate:MMM dd, yyyy HH:mm} | Priority: {taskPriority}",
            PerformedByUserId = user.Id,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Task '{title}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Leads/CompleteTask
    [HttpPost]
    public async Task<IActionResult> CompleteTask(int taskId, int leadId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var task = await _context.CrmTasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.TenantId == user.TenantId);

        if (task == null)
        {
            TempData["ErrorMessage"] = "Task not found.";
            return RedirectToAction(nameof(Details), new { id = leadId });
        }

        task.Status = CrmTaskStatus.Completed;
        task.CompletedAt = DateTime.UtcNow;

        // Log activity
        _context.LeadActivities.Add(new LeadActivity
        {
            LeadId = leadId,
            TenantId = user.TenantId,
            Type = ActivityType.Task,
            Title = $"Task completed: {task.Title}",
            PerformedByUserId = user.Id,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Task marked as completed.";
        return RedirectToAction(nameof(Details), new { id = leadId });
    }

    // GET: Leads/ExportCsv
    public async Task<IActionResult> ExportCsv()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var leads = await _context.Leads
            .Where(l => l.TenantId == user.TenantId)
            .Include(l => l.Popup)
            .OrderByDescending(l => l.CapturedAt)
            .ToListAsync();

        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Date,Email,First Name,Last Name,Phone,Company,Job Title,Source,Popup,Status,Score,UTM Source,UTM Campaign,Notes");

        foreach (var lead in leads)
        {
            csv.AppendLine($"{lead.CapturedAt:yyyy-MM-dd HH:mm:ss}," +
                          $"\"{EscapeCsv(lead.Email)}\"," +
                          $"\"{EscapeCsv(lead.FirstName)}\"," +
                          $"\"{EscapeCsv(lead.LastName)}\"," +
                          $"\"{EscapeCsv(lead.Phone)}\"," +
                          $"\"{EscapeCsv(lead.Company)}\"," +
                          $"\"{EscapeCsv(lead.JobTitle)}\"," +
                          $"\"{EscapeCsv(lead.LeadSource)}\"," +
                          $"\"{EscapeCsv(lead.Popup?.Name)}\"," +
                          $"{lead.Status}," +
                          $"{lead.Score}," +
                          $"\"{EscapeCsv(lead.UtmSource)}\"," +
                          $"\"{EscapeCsv(lead.UtmCampaign)}\"," +
                          $"\"{EscapeCsv(lead.Notes)}\"");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"leads_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    // GET: Leads/ExportExcel - Same as CSV for now
    public async Task<IActionResult> ExportExcel()
    {
        return await ExportCsv();
    }

    private string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", "");
    }

    // POST: Leads/Delete/5
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == user.TenantId);

        if (lead == null)
        {
            return NotFound();
        }

        _context.Leads.Remove(lead);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Lead deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Leads/Export
    public async Task<IActionResult> Export(int? popupId, string? status)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var query = _context.Leads
            .Where(l => l.TenantId == user.TenantId)
            .Include(l => l.Popup)
            .AsQueryable();

        if (popupId.HasValue)
        {
            query = query.Where(l => l.PopupId == popupId.Value);
        }

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<LeadStatus>(status, out var leadStatus))
        {
            query = query.Where(l => l.Status == leadStatus);
        }

        var leads = await query.OrderByDescending(l => l.CapturedAt).ToListAsync();

        // Generate CSV
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Date,Email,First Name,Last Name,Phone,Company,Source,Popup,Status,UTM Source,UTM Campaign");

        foreach (var lead in leads)
        {
            csv.AppendLine($"{lead.CapturedAt:yyyy-MM-dd HH:mm:ss}," +
                          $"\"{lead.Email}\"," +
                          $"\"{lead.FirstName}\"," +
                          $"\"{lead.LastName}\"," +
                          $"\"{lead.Phone}\"," +
                          $"\"{lead.Company}\"," +
                          $"\"{lead.Source}\"," +
                          $"\"{lead.Popup?.Name}\"," +
                          $"{lead.Status}," +
                          $"\"{lead.UtmSource}\"," +
                          $"\"{lead.UtmCampaign}\"");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"leads_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    // API: Capture lead from popup
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Capture([FromBody] LeadCaptureRequest request)
    {
        if (request == null || request.PopupId <= 0)
        {
            return BadRequest(new { success = false, message = "Invalid request" });
        }

        var popup = await _context.Popups
            .Include(p => p.Tenant)
            .FirstOrDefaultAsync(p => p.Id == request.PopupId);

        if (popup == null)
        {
            return NotFound(new { success = false, message = "Popup not found" });
        }

        // Create lead
        var lead = new Lead
        {
            PopupId = request.PopupId,
            TenantId = popup.TenantId,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            Company = request.Company,
            CustomFields = JsonSerializer.Serialize(request.CustomFields ?? new Dictionary<string, object>()),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers["User-Agent"].ToString(),
            Source = request.Source,
            Referrer = Request.Headers["Referer"].ToString(),
            UtmSource = request.UtmSource,
            UtmMedium = request.UtmMedium,
            UtmCampaign = request.UtmCampaign,
            UtmTerm = request.UtmTerm,
            UtmContent = request.UtmContent,
            ConsentGiven = request.ConsentGiven,
            ConsentDate = request.ConsentGiven ? DateTime.UtcNow : null,
            ConsentText = request.ConsentText,
            ViewType = request.ViewType,
            Status = LeadStatus.New
        };

        _context.Leads.Add(lead);
        
        // Update popup conversions
        popup.Conversions++;
        
        await _context.SaveChangesAsync();

        return Ok(new { success = true, leadId = lead.Id, message = "Lead captured successfully" });
    }
}

// Request model for lead capture
public class LeadCaptureRequest
{
    public int PopupId { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public Dictionary<string, object>? CustomFields { get; set; }
    public string? Source { get; set; }
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }
    public string? UtmTerm { get; set; }
    public string? UtmContent { get; set; }
    public bool ConsentGiven { get; set; }
    public string? ConsentText { get; set; }
    public string? ViewType { get; set; }
}
