using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRUD_ChildrenCare.Data;
using CRUD_ChildrenCare.Models;
using CRUD_ChildrenCare.Areas.Admin.ViewModels;

namespace CRUD_ChildrenCare.Areas.Admin.Controllers;

/// <summary>
/// Controller quản lý bài viết (Posts) - dành cho Admin
/// [PART 2 - CONTROLLER LAYER - ADMIN]
/// </summary>
[Area("Admin")]
[Authorize(Roles = "Admin,Manager")]
public class PostsController : Controller
{
    private readonly ApplicationDbContext _context;

    public PostsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// GET: /Admin/Posts - Danh sách tất cả bài viết
    /// Use Case: Admin xem danh sách bài viết
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? search, PostStatus? status)
    {
        var query = _context.Posts.Include(p => p.Author).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Title.Contains(search));

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        var posts = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

        var viewModel = new PostListViewModel
        {
            Posts = posts,
            SearchKeyword = search,
            FilterStatus = status
        };

        return View(viewModel);
    }

    /// <summary>
    /// GET: /Admin/Posts/Create - Form tạo bài viết mới
    /// Use Case: Admin tạo bài viết mới
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        return View(new PostFormViewModel());
    }

    /// <summary>
    /// POST: /Admin/Posts/Create - Lưu bài viết mới vào DB
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PostFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var post = new Post
        {
            Title = model.Title,
            Summary = model.Summary,
            Content = model.Content,
            ThumbnailUrl = model.ThumbnailUrl,
            IsFeatured = model.IsFeatured,
            Status = model.Status,
            CreatedAt = DateTime.UtcNow
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Tạo bài viết thành công!";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// GET: /Admin/Posts/Edit/{id} - Form chỉnh sửa bài viết
    /// Use Case: Admin chỉnh sửa bài viết
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post is null) return NotFound();

        var model = new PostFormViewModel
        {
            Id = post.Id,
            Title = post.Title,
            Summary = post.Summary,
            Content = post.Content,
            ThumbnailUrl = post.ThumbnailUrl,
            IsFeatured = post.IsFeatured,
            Status = post.Status
        };

        return View(model);
    }

    /// <summary>
    /// POST: /Admin/Posts/Edit/{id} - Cập nhật bài viết vào DB
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PostFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var post = await _context.Posts.FindAsync(id);
        if (post is null) return NotFound();

        post.Title = model.Title;
        post.Summary = model.Summary;
        post.Content = model.Content;
        post.ThumbnailUrl = model.ThumbnailUrl;
        post.IsFeatured = model.IsFeatured;
        post.Status = model.Status;
        post.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Cập nhật bài viết thành công!";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// POST: /Admin/Posts/ToggleStatus/{id} - Bật/Tắt trạng thái bài viết
    /// Use Case: Admin thay đổi trạng thái bài viết
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post is null) return NotFound();

        post.Status = post.Status == PostStatus.Published
            ? PostStatus.Draft
            : PostStatus.Published;

        post.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
