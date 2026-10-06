using System.Security.Claims;
using CRUD_ChildrenCare.Areas.Admin.ViewModels.MedicalExaminations;
using CRUD_ChildrenCare.Data;
using CRUD_ChildrenCare.Services.MedicalExaminations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRUD_ChildrenCare.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SystemData.RoleValues.Doctor + "," + SystemData.RoleValues.Nurse + "," + SystemData.RoleValues.Manager + "," + SystemData.RoleValues.Admin)]
public sealed class MedicalExaminationsController(
    IMedicalExaminationService examinationService,
    ApplicationDbContext dbContext) : Controller
{
    private const int PageSize = 10;

    private string CurrentUserRole =>
        User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    private int CurrentUserId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    private bool IsDoctorOrAdmin =>
        User.IsInRole(SystemData.RoleValues.Doctor) || User.IsInRole(SystemData.RoleValues.Admin);

    private bool IsStaffRestricted =>
        User.IsInRole(SystemData.RoleValues.Doctor) || User.IsInRole(SystemData.RoleValues.Nurse);

    [HttpGet]
    public async Task<IActionResult> Index(
        int? serviceId, DateTime? fromDate, DateTime? toDate, string? medicineName, string? search, int page = 1)
    {
        var (exams, totalItems, totalPages) = await examinationService.GetExaminationsAsync(
            serviceId, fromDate, toDate, medicineName, search,
            null, IsStaffRestricted, CurrentUserId, page, PageSize, HttpContext.RequestAborted);

        var services = await dbContext.Services
            .OrderBy(s => s.Title)
            .ToListAsync(HttpContext.RequestAborted);

        return View(new MedicalExaminationListViewModel
        {
            Examinations = exams,
            Services = services,
            ServiceId = serviceId,
            FromDate = fromDate,
            ToDate = toDate,
            MedicineName = medicineName,
            SearchQuery = search,
            PageIndex = page,
            TotalPages = totalPages,
            TotalItems = totalItems,
            CanCreate = IsDoctorOrAdmin
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var exam = await examinationService.GetExaminationByIdAsync(
            id, CurrentUserId, CurrentUserRole, HttpContext.RequestAborted);

        if (exam == null)
        {
            return NotFound();
        }

        return View(new MedicalExaminationDetailsViewModel
        {
            Examination = exam,
            CanCreate = IsDoctorOrAdmin
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? reservationId)
    {
        if (!IsDoctorOrAdmin)
        {
            return Forbid();
        }

        var eligibleReservations = await examinationService.GetEligibleReservationsAsync(
            CurrentUserId, CurrentUserRole, HttpContext.RequestAborted);

        var doctorName = User.Identity?.Name ?? "Bác sĩ";

        var model = new CreateMedicalExaminationViewModel
        {
            ReservationId = reservationId ?? 0,
            DoctorId = CurrentUserId,
            DoctorName = doctorName,
            ExaminationDate = DateTime.Now,
            EligibleReservations = eligibleReservations,
            Items =
            [
                new PrescriptionItemInputViewModel { MedicineName = "", Dosage = "", Quantity = 1, Usage = "" }
            ]
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMedicalExaminationViewModel model)
    {
        if (!IsDoctorOrAdmin)
        {
            return Forbid();
        }

        // Clean out empty rows
        model.Items = model.Items.Where(i => !string.IsNullOrWhiteSpace(i.MedicineName)).ToList();

        if (model.Items.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Đơn thuốc bắt buộc phải có ít nhất 1 loại thuốc.");
        }

        if (model.ExaminationDate > DateTime.Now.AddMinutes(5))
        {
            ModelState.AddModelError(nameof(model.ExaminationDate), "Thời gian khám không được sau thời điểm hiện tại.");
        }

        if (!ModelState.IsValid)
        {
            model.EligibleReservations = await examinationService.GetEligibleReservationsAsync(
                CurrentUserId, CurrentUserRole, HttpContext.RequestAborted);
            if (model.Items.Count == 0)
            {
                model.Items.Add(new PrescriptionItemInputViewModel());
            }
            return View(model);
        }

        var (success, errorMessage) = await examinationService.CreateExaminationAsync(
            model, CurrentUserId, HttpContext.RequestAborted);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Không thể lưu hồ sơ khám bệnh.");
            model.EligibleReservations = await examinationService.GetEligibleReservationsAsync(
                CurrentUserId, CurrentUserRole, HttpContext.RequestAborted);
            return View(model);
        }

        TempData["SuccessMessage"] = "Hồ sơ khám bệnh và đơn thuốc đã được tạo thành công. Đặt lịch đã chuyển sang hoàn tất (Success).";
        return RedirectToAction(nameof(Index));
    }
}
