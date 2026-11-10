using CRUD_ChildrenCare.Models;

namespace CRUD_ChildrenCare.ViewModels;

/// <summary>
/// ViewModel cho trang danh sách blog (người dùng công khai)
/// [PART 2 - VIEWMODEL - PUBLIC]
/// </summary>
public class BlogListViewModel
{
    public List<Post> Posts { get; set; } = new();
    public string? SearchKeyword { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}
