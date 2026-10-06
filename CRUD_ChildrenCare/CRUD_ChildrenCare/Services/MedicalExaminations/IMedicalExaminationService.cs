using CRUD_ChildrenCare.Areas.Admin.ViewModels.MedicalExaminations;
using CRUD_ChildrenCare.Models;

namespace CRUD_ChildrenCare.Services.MedicalExaminations;

public interface IMedicalExaminationService
{
    Task<(IReadOnlyList<MedicalExamination> Examinations, int TotalItems, int TotalPages)> GetExaminationsAsync(
        int? serviceId,
        DateTime? fromDate,
        DateTime? toDate,
        string? medicineName,
        string? search,
        int? doctorId,
        bool isStaffRestricted,
        int currentUserId,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default);

    Task<MedicalExamination?> GetExaminationByIdAsync(int id, int currentUserId, string currentUserRole, CancellationToken ct = default);

    Task<IReadOnlyList<Reservation>> GetEligibleReservationsAsync(int currentUserId, string currentUserRole, CancellationToken ct = default);

    Task<(bool Success, string? ErrorMessage)> CreateExaminationAsync(CreateMedicalExaminationViewModel model, int doctorId, CancellationToken ct = default);
}
