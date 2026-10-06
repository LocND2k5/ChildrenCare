using System.ComponentModel.DataAnnotations;
using CRUD_ChildrenCare.Models;
using CRUD_ChildrenCare.Models.Enums;
using Microsoft.AspNetCore.Http;

namespace CRUD_ChildrenCare.ViewModels.Feedback;

public sealed class CreateFeedbackViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên từ 2 đến 100 ký tự.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn giới tính.")]
    public Gender Gender { get; set; } = Gender.Female;

    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
    [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm đúng 10 chữ số và bắt đầu bằng 0.")]
    public string Mobile { get; set; } = string.Empty;

    public int? ServiceId { get; set; }

    [Required(ErrorMessage = "Vui lòng đánh giá số sao.")]
    [Range(1, 5, ErrorMessage = "Đánh giá từ 1 đến 5 sao.")]
    public int RatedStar { get; set; } = 5;

    [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Nội dung phản hồi từ 10 đến 1000 ký tự.")]
    public string Content { get; set; } = string.Empty;

    public List<IFormFile>? Images { get; set; }

    public IReadOnlyList<Service> AvailableServices { get; set; } = [];
}
