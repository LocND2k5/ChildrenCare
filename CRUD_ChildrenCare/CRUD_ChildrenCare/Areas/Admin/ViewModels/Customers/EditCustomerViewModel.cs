using System.ComponentModel.DataAnnotations;
using CRUD_ChildrenCare.Models.Enums;

namespace CRUD_ChildrenCare.Areas.Admin.ViewModels.Customers;

public sealed class EditCustomerViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên từ 2 đến 100 ký tự.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn giới tính.")]
    public Gender Gender { get; set; }

    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm đúng 10 chữ số và bắt đầu bằng 0.")]
    public string Mobile { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự.")]
    public string? Address { get; set; }

    public ContactStatus Status { get; set; }
}
