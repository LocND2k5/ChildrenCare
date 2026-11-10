using System.ComponentModel.DataAnnotations;
using CRUD_ChildrenCare.Models;

namespace CRUD_ChildrenCare.Areas.Admin.ViewModels;

/// <summary>
/// ViewModel cho form tạo/sửa Slider - Admin
/// [PART 2 - VIEWMODEL - ADMIN]
/// </summary>
public class SliderFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    [MaxLength(200)]
    [Display(Name = "Tiêu đề")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "URL ảnh không được để trống")]
    [MaxLength(300)]
    [Display(Name = "URL ảnh")]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(300)]
    [Display(Name = "Link khi click vào")]
    public string? LinkUrl { get; set; }

    [Display(Name = "Thứ tự hiển thị")]
    public int DisplayOrder { get; set; } = 0;

    [Display(Name = "Trạng thái")]
    public SliderStatus Status { get; set; } = SliderStatus.Active;
}
