using System.ComponentModel.DataAnnotations;
using CRUD_ChildrenCare.Models;

namespace CRUD_ChildrenCare.Areas.Admin.ViewModels;

/// <summary>
/// ViewModel cho form tạo/sửa bài viết - Admin
/// [PART 2 - VIEWMODEL - ADMIN]
/// </summary>
public class PostFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    [MaxLength(200, ErrorMessage = "Tiêu đề tối đa 200 ký tự")]
    [Display(Name = "Tiêu đề")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tóm tắt không được để trống")]
    [MaxLength(500, ErrorMessage = "Tóm tắt tối đa 500 ký tự")]
    [Display(Name = "Tóm tắt")]
    public string Summary { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nội dung không được để trống")]
    [Display(Name = "Nội dung")]
    public string Content { get; set; } = string.Empty;

    [MaxLength(300)]
    [Display(Name = "Ảnh thumbnail (URL)")]
    public string? ThumbnailUrl { get; set; }

    [Display(Name = "Nổi bật?")]
    public bool IsFeatured { get; set; } = false;

    [Display(Name = "Trạng thái")]
    public PostStatus Status { get; set; } = PostStatus.Draft;
}
