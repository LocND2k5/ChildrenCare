using CRUD_ChildrenCare.Areas.Admin.ViewModels.Feedbacks;
using CRUD_ChildrenCare.Data;
using CRUD_ChildrenCare.Models.Enums;
using CRUD_ChildrenCare.Services.Feedbacks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRUD_ChildrenCare.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SystemData.RoleValues.Manager + "," + SystemData.RoleValues.Admin + "," + SystemData.RoleValues.Doctor + "," + SystemData.RoleValues.Nurse)]
public sealed class FeedbacksController(
    IFeedbackService feedbackService,
    ApplicationDbContext dbContext) : Controller
{
    private const int PageSize = 10;

    private bool IsManagerOrAdmin =>
        User.IsInRole(SystemData.RoleValues.Manager) || User.IsInRole(SystemData.RoleValues.Admin);

    [HttpGet]
    public async Task<IActionResult> Index(
        string? search, FeedbackStatus? status, int? serviceId, int? ratedStar, string? sort, int page = 1)
    {
        var (feedbacks, totalItems, totalPages) = await feedbackService.GetFeedbacksAsync(
            search, status, serviceId, ratedStar, sort, page, PageSize, HttpContext.RequestAborted);

        var services = await dbContext.Services
            .OrderBy(s => s.Title)
            .ToListAsync(HttpContext.RequestAborted);

        return View(new FeedbackListAdminViewModel
        {
            Feedbacks = feedbacks,
            Services = services,
            SearchQuery = search,
            Status = status,
            ServiceId = serviceId,
            RatedStar = ratedStar,
            Sort = sort,
            PageIndex = page,
            TotalPages = totalPages,
            TotalItems = totalItems,
            CanManage = IsManagerOrAdmin
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var feedback = await feedbackService.GetFeedbackByIdAsync(id, HttpContext.RequestAborted);
        if (feedback == null)
        {
            return NotFound();
        }

        return View(new FeedbackDetailsAdminViewModel
        {
            Feedback = feedback,
            CanManage = IsManagerOrAdmin
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        if (!IsManagerOrAdmin)
        {
            return Forbid();
        }

        var success = await feedbackService.ToggleStatusAsync(id, HttpContext.RequestAborted);
        if (!success)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Cập nhật trạng thái hiển thị phản hồi thành công.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, FeedbackStatus status)
    {
        if (!IsManagerOrAdmin)
        {
            return Forbid();
        }

        var success = await feedbackService.UpdateStatusAsync(id, status, HttpContext.RequestAborted);
        if (!success)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Cập nhật trạng thái phản hồi thành công.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
