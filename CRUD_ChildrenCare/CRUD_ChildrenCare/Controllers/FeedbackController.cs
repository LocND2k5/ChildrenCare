using System.Security.Claims;
using CRUD_ChildrenCare.Data;
using CRUD_ChildrenCare.Models.Enums;
using CRUD_ChildrenCare.Services.Feedbacks;
using CRUD_ChildrenCare.ViewModels.Feedback;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRUD_ChildrenCare.Controllers;

public sealed class FeedbackController(
    IFeedbackService feedbackService,
    ApplicationDbContext dbContext) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(int? serviceId)
    {
        var feedbacks = await feedbackService.GetPublishedFeedbacksForServiceAsync(serviceId, 30);
        ViewBag.ServiceId = serviceId;
        ViewBag.Services = await dbContext.Services
            .Where(s => s.Status == ServiceStatus.Active)
            .OrderBy(s => s.Title)
            .ToListAsync();

        return View(feedbacks);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? serviceId)
    {
        var model = new CreateFeedbackViewModel
        {
            ServiceId = serviceId,
            AvailableServices = await dbContext.Services
                .Where(s => s.Status == ServiceStatus.Active)
                .OrderBy(s => s.Title)
                .ToListAsync()
        };

        if (User.Identity?.IsAuthenticated == true)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user != null)
                {
                    model.FullName = user.FullName;
                    model.Email = user.Email;
                    model.Mobile = user.Mobile;
                    model.Gender = user.Gender;
                }
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateFeedbackViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableServices = await dbContext.Services
                .Where(s => s.Status == ServiceStatus.Active)
                .OrderBy(s => s.Title)
                .ToListAsync();
            return View(model);
        }

        try
        {
            await feedbackService.CreateFeedbackAsync(model);
            return RedirectToAction(nameof(Success));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi gửi phản hồi: " + ex.Message);
            model.AvailableServices = await dbContext.Services
                .Where(s => s.Status == ServiceStatus.Active)
                .OrderBy(s => s.Title)
                .ToListAsync();
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Success()
    {
        return View();
    }
}
