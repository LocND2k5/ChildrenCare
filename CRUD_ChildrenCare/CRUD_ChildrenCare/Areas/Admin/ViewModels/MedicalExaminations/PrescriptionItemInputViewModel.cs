using System.ComponentModel.DataAnnotations;

namespace CRUD_ChildrenCare.Areas.Admin.ViewModels.MedicalExaminations;

public sealed class PrescriptionItemInputViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên thuốc.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Tên thuốc từ 2 đến 150 ký tự.")]
    public string MedicineName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập liều lượng.")]
    [StringLength(100, ErrorMessage = "Liều lượng tối đa 100 ký tự.")]
    public string Dosage { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số lượng.")]
    [Range(1, 1000, ErrorMessage = "Số lượng thuốc từ 1 trở lên.")]
    public int Quantity { get; set; } = 1;

    [StringLength(255, ErrorMessage = "Cách dùng tối đa 255 ký tự.")]
    public string? Usage { get; set; }
}
