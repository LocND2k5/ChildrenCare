using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRUD_ChildrenCare.Data;
using CRUD_ChildrenCare.Models;
using CRUD_ChildrenCare.ViewModels;

namespace CRUD_ChildrenCare.Controllers;

/// <summary>
/// Controller xử lý trang chủ - hiển thị Slider và bài viết nổi bật
/// [PART 2 - CONTROLLER LAYER - PUBLIC]
/// </summary>
[AllowAnonymous]
public sealed class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// GET: / - Trang chủ: load Slider đang Active + bài viết Featured đã Published
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var sliders = await _context.Sliders
            .Where(s => s.Status == SliderStatus.Active)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();

        var featuredPosts = await _context.Posts
            .Include(p => p.Author)
            .Where(p => p.Status == PostStatus.Published && p.IsFeatured)
            .OrderByDescending(p => p.CreatedAt)
            .Take(6)
            .ToListAsync();

        var viewModel = new HomeViewModel
        {
            Sliders = sliders,
            FeaturedPosts = featuredPosts
        };

        return View(viewModel);
    }
}
