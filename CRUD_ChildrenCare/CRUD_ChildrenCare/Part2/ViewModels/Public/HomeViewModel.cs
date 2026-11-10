using CRUD_ChildrenCare.Models;

namespace CRUD_ChildrenCare.ViewModels;

/// <summary>
/// ViewModel cho trang chủ - chứa dữ liệu Slider + bài viết nổi bật
/// [PART 2 - VIEWMODEL - PUBLIC]
/// </summary>
public class HomeViewModel
{
    public List<Slider> Sliders { get; set; } = new();
    public List<Post> FeaturedPosts { get; set; } = new();
}
