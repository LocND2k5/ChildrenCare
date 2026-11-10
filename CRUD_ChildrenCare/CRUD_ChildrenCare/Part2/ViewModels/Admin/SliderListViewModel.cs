using CRUD_ChildrenCare.Models;

namespace CRUD_ChildrenCare.Areas.Admin.ViewModels;

/// <summary>
/// ViewModel cho trang danh sách Slider - Admin
/// [PART 2 - VIEWMODEL - ADMIN]
/// </summary>
public class SliderListViewModel
{
    public List<Slider> Sliders { get; set; } = new();
}
