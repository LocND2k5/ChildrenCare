using CRUD_ChildrenCare.Areas.Admin.ViewModels.MedicalExaminations;
using CRUD_ChildrenCare.Data;
using CRUD_ChildrenCare.Models;
using CRUD_ChildrenCare.Models.Enums;
using CRUD_ChildrenCare.Services.Contacts;
using Microsoft.EntityFrameworkCore;

namespace CRUD_ChildrenCare.Services.MedicalExaminations;

public sealed class MedicalExaminationService(
    ApplicationDbContext dbContext,
    IContactService contactService) : IMedicalExaminationService
{
    public async Task<(IReadOnlyList<MedicalExamination> Examinations, int TotalItems, int TotalPages)> GetExaminationsAsync(
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
        CancellationToken ct = default)
    {
        var query = dbContext.MedicalExaminations
            .Include(m => m.Reservation)
                .ThenInclude(r => r.ReservationItems)
                    .ThenInclude(ri => ri.Service)
            .Include(m => m.Doctor)
            .Include(m => m.Prescriptions)
                .ThenInclude(p => p.PrescriptionItems)
            .AsQueryable();

        if (isStaffRestricted)
        {
            query = query.Where(m => m.DoctorId == currentUserId || m.Reservation.AssignedStaffId == currentUserId);
        }
        else if (doctorId.HasValue)
        {
            query = query.Where(m => m.DoctorId == doctorId.Value);
        }

        if (serviceId.HasValue)
        {
            query = query.Where(m => m.Reservation.ReservationItems.Any(ri => ri.ServiceId == serviceId.Value));
        }

        if (fromDate.HasValue)
        {
            query = query.Where(m => m.ExaminationDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(m => m.ExaminationDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));
        }

        if (!string.IsNullOrWhiteSpace(medicineName))
        {
            var med = medicineName.Trim().ToLower();
            query = query.Where(m => m.Prescriptions.Any(p => p.PrescriptionItems.Any(pi => pi.MedicineName.ToLower().Contains(med))));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(m => m.Reservation.ReceiverFullName.Contains(s) || m.ReservationId.ToString() == s);
        }

        var totalItems = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        if (pageIndex < 1) pageIndex = 1;
        if (pageIndex > totalPages && totalPages > 0) pageIndex = totalPages;

        var exams = await query
            .OrderByDescending(m => m.ExaminationDate)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (exams, totalItems, totalPages);
    }

    public async Task<MedicalExamination?> GetExaminationByIdAsync(int id, int currentUserId, string currentUserRole, CancellationToken ct = default)
    {
        var query = dbContext.MedicalExaminations
            .Include(m => m.Reservation)
                .ThenInclude(r => r.ReservationItems)
                    .ThenInclude(ri => ri.Service)
            .Include(m => m.Doctor)
            .Include(m => m.Prescriptions)
                .ThenInclude(p => p.PrescriptionItems)
            .Where(m => m.Id == id);

        if (currentUserRole is "Doctor" or "Nurse")
        {
            query = query.Where(m => m.DoctorId == currentUserId || m.Reservation.AssignedStaffId == currentUserId);
        }

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Reservation>> GetEligibleReservationsAsync(int currentUserId, string currentUserRole, CancellationToken ct = default)
    {
        var query = dbContext.Reservations
            .Include(r => r.ReservationItems).ThenInclude(i => i.Service)
            .Where(r => r.Status == ReservationStatus.Submitted || r.Status == ReservationStatus.Success);

        if (currentUserRole is "Doctor")
        {
            query = query.Where(r => r.AssignedStaffId == currentUserId);
        }

        return await query
            .OrderByDescending(r => r.CheckupTime)
            .Take(50)
            .ToListAsync(ct);
    }

    public async Task<(bool Success, string? ErrorMessage)> CreateExaminationAsync(CreateMedicalExaminationViewModel model, int doctorId, CancellationToken ct = default)
    {
        if (model.Items == null || model.Items.Count == 0 || model.Items.All(i => string.IsNullOrWhiteSpace(i.MedicineName)))
        {
            return (false, "Đơn thuốc phải có ít nhất 1 dòng thuốc.");
        }

        if (model.ExaminationDate > DateTime.UtcNow.AddMinutes(5))
        {
            return (false, "Thời gian khám không được sau thời điểm hiện tại.");
        }

        var reservation = await dbContext.Reservations.FindAsync([model.ReservationId], ct);
        if (reservation == null)
        {
            return (false, "Không tìm thấy thông tin đặt lịch.");
        }

        var exam = new MedicalExamination
        {
            ReservationId = reservation.Id,
            DoctorId = doctorId,
            ExaminationDate = model.ExaminationDate,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        var prescription = new Prescription
        {
            Diagnosis = model.Diagnosis.Trim(),
            Note = model.Note?.Trim(),
            CreatedDate = DateTime.UtcNow
        };

        foreach (var item in model.Items.Where(i => !string.IsNullOrWhiteSpace(i.MedicineName)))
        {
            prescription.PrescriptionItems.Add(new PrescriptionItem
            {
                MedicineName = item.MedicineName.Trim(),
                Dosage = item.Dosage.Trim(),
                Quantity = item.Quantity > 0 ? item.Quantity : 1,
                Usage = item.Usage?.Trim()
            });
        }

        exam.Prescriptions.Add(prescription);
        dbContext.MedicalExaminations.Add(exam);

        // Mark Reservation as Success
        if (reservation.Status != ReservationStatus.Success)
        {
            reservation.Status = ReservationStatus.Success;
            reservation.UpdatedDate = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(ct);

        // Promote Contact to Customer status
        await contactService.PromoteToCustomerIfSuccessAsync(reservation.ReceiverEmail, ct);

        return (true, null);
    }
}
