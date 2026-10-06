using CRUD_ChildrenCare.Data;
using CRUD_ChildrenCare.Models;
using CRUD_ChildrenCare.Models.Enums;
using CRUD_ChildrenCare.Services.Contacts;
using CRUD_ChildrenCare.Validation;
using CRUD_ChildrenCare.ViewModels.Feedback;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace CRUD_ChildrenCare.Services.Feedbacks;

public sealed class FeedbackService(
    ApplicationDbContext dbContext,
    IContactService contactService,
    IWebHostEnvironment environment) : IFeedbackService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };
    private const long MaxFileBytes = 2 * 1024 * 1024; // 2 MB

    public async Task<Feedback> CreateFeedbackAsync(CreateFeedbackViewModel model, CancellationToken ct = default)
    {
        var feedback = new Feedback
        {
            FullName = AccountValidation.NormalizeFullName(model.FullName),
            Gender = model.Gender,
            Email = model.Email.Trim(),
            Mobile = model.Mobile.Trim(),
            ServiceId = model.ServiceId,
            RatedStar = Math.Clamp(model.RatedStar, 1, 5),
            Content = model.Content.Trim(),
            Status = FeedbackStatus.Published,
            CreatedDate = DateTime.UtcNow
        };

        dbContext.Feedbacks.Add(feedback);
        await dbContext.SaveChangesAsync(ct);

        // Process images (up to 5)
        if (model.Images != null && model.Images.Count > 0)
        {
            var uploadDir = Path.Combine(environment.WebRootPath, "uploads", "feedbacks");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            int count = 0;
            foreach (var file in model.Images)
            {
                if (count >= 5) break;
                if (file.Length == 0 || file.Length > MaxFileBytes) continue;

                var ext = Path.GetExtension(file.FileName);
                if (!AllowedExtensions.Contains(ext)) continue;

                var fileName = $"{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream, ct);
                }

                var imgUrl = $"/uploads/feedbacks/{fileName}";
                dbContext.FeedbackImages.Add(new FeedbackImage
                {
                    FeedbackId = feedback.Id,
                    ImageUrl = imgUrl
                });
                count++;
            }

            await dbContext.SaveChangesAsync(ct);
        }

        // Sync Contact
        await contactService.SyncContactFromFeedbackAsync(
            feedback.FullName,
            feedback.Gender,
            feedback.Email,
            feedback.Mobile,
            ct);

        return feedback;
    }

    public async Task<(IReadOnlyList<Feedback> Feedbacks, int TotalItems, int TotalPages)> GetFeedbacksAsync(
        string? search, FeedbackStatus? status, int? serviceId, int? ratedStar, string? sort, int pageIndex, int pageSize, CancellationToken ct = default)
    {
        var query = dbContext.Feedbacks
            .Include(f => f.Service)
            .Include(f => f.FeedbackImages)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(f => f.FullName.Contains(s) || f.Content.Contains(s));
        }

        if (status.HasValue)
        {
            query = query.Where(f => f.Status == status.Value);
        }

        if (serviceId.HasValue)
        {
            query = query.Where(f => f.ServiceId == serviceId.Value);
        }

        if (ratedStar.HasValue)
        {
            query = query.Where(f => f.RatedStar == ratedStar.Value);
        }

        query = sort switch
        {
            "FullName" => query.OrderBy(f => f.FullName),
            "FullNameDesc" => query.OrderByDescending(f => f.FullName),
            "Service" => query.OrderBy(f => f.Service != null ? f.Service.Title : string.Empty),
            "RatedStar" => query.OrderBy(f => f.RatedStar),
            "RatedStarDesc" => query.OrderByDescending(f => f.RatedStar),
            "Status" => query.OrderBy(f => f.Status),
            _ => query.OrderByDescending(f => f.CreatedDate)
        };

        var totalItems = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        if (pageIndex < 1) pageIndex = 1;
        if (pageIndex > totalPages && totalPages > 0) pageIndex = totalPages;

        var feedbacks = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (feedbacks, totalItems, totalPages);
    }

    public async Task<Feedback?> GetFeedbackByIdAsync(int id, CancellationToken ct = default)
    {
        return await dbContext.Feedbacks
            .Include(f => f.Service)
            .Include(f => f.FeedbackImages)
            .FirstOrDefaultAsync(f => f.Id == id, ct);
    }

    public async Task<bool> ToggleStatusAsync(int id, CancellationToken ct = default)
    {
        var feedback = await dbContext.Feedbacks.FindAsync([id], ct);
        if (feedback == null) return false;

        feedback.Status = feedback.Status == FeedbackStatus.Published
            ? FeedbackStatus.Hidden
            : FeedbackStatus.Published;

        await dbContext.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UpdateStatusAsync(int id, FeedbackStatus status, CancellationToken ct = default)
    {
        var feedback = await dbContext.Feedbacks.FindAsync([id], ct);
        if (feedback == null) return false;

        feedback.Status = status;
        await dbContext.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<Feedback>> GetPublishedFeedbacksForServiceAsync(int? serviceId, int limit = 10, CancellationToken ct = default)
    {
        var query = dbContext.Feedbacks
            .Include(f => f.FeedbackImages)
            .Where(f => f.Status == FeedbackStatus.Published);

        if (serviceId.HasValue)
        {
            query = query.Where(f => f.ServiceId == serviceId.Value);
        }

        return await query
            .OrderByDescending(f => f.CreatedDate)
            .Take(limit)
            .ToListAsync(ct);
    }
}
