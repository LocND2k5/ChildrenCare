using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRUD_ChildrenCare.Data;
using CRUD_ChildrenCare.Models;
using CRUD_ChildrenCare.ViewModels;

namespace CRUD_ChildrenCare.Controllers;

/// <summary>
/// Controller xử lý trang blog công khai (khách hàng/người dùng xem bài viết)
/// [PART 2 - CONTROLLER LAYER - PUBLIC]
/// </summary>
[AllowAnonymous]
public sealed class BlogsController : Controller
{
    private readonly ApplicationDbContext _context;

    public BlogsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// GET: /Blogs - Danh sách bài viết đã Published (có tìm kiếm)
    /// Use Case: Customer xem danh sách bài viết
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        const int pageSize = 6;

        var query = _context.Posts
            .Include(p => p.Author)
            .Where(p => p.Status == PostStatus.Published);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Title.Contains(search) || p.Summary.Contains(search));

        var totalItems = await query.CountAsync();
        var posts = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var viewModel = new BlogListViewModel
        {
            Posts = posts,
            SearchKeyword = search,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };

        return View(viewModel);
    }

    /// <summary>
    /// GET: /Blogs/Details/{id} - Xem chi tiết một bài viết
    /// Use Case: Customer xem chi tiết bài viết
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var post = await _context.Posts
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == id && p.Status == PostStatus.Published);

        if (post is null)
            return NotFound();

        return View(post);
    }
}
