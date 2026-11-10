using CRUD_ChildrenCare.Models;

namespace CRUD_ChildrenCare.Areas.Admin.ViewModels;

/// <summary>
/// ViewModel cho trang danh sách bài viết - Admin
/// [PART 2 - VIEWMODEL - ADMIN]
/// </summary>
public class PostListViewModel
{
    public List<Post> Posts { get; set; } = new();
    public string? SearchKeyword { get; set; }
    public PostStatus? FilterStatus { get; set; }
}
