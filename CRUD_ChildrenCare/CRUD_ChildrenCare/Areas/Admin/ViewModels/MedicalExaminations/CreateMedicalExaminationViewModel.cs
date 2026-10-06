using System.ComponentModel.DataAnnotations;
using CRUD_ChildrenCare.Models;

namespace CRUD_ChildrenCare.Areas.Admin.ViewModels.MedicalExaminations;

public sealed class CreateMedicalExaminationViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn đặt lịch khám.")]
    public int ReservationId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập ngày giờ khám.")]
    public DateTime ExaminationDate { get; set; } = DateTime.UtcNow;

    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập chẩn đoán bệnh.")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Chẩn đoán bệnh từ 5 đến 500 ký tự.")]
    public string Diagnosis { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự.")]
    public string? Note { get; set; }

    public List<PrescriptionItemInputViewModel> Items { get; set; } = [];

    public IReadOnlyList<Reservation> EligibleReservations { get; set; } = [];
}
