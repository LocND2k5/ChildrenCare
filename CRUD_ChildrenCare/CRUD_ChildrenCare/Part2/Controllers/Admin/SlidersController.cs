using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRUD_ChildrenCare.Data;
using CRUD_ChildrenCare.Models;
using CRUD_ChildrenCare.Areas.Admin.ViewModels;

namespace CRUD_ChildrenCare.Areas.Admin.Controllers;

/// <summary>
/// Controller quản lý Slider ảnh trang chủ - dành cho Admin
/// [PART 2 - CONTROLLER LAYER - ADMIN]
/// </summary>
[Area("Admin")]
[Authorize(Roles = "Admin,Manager")]
public class SlidersController : Controller
{
    private readonly ApplicationDbContext _context;

    public SlidersController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// GET: /Admin/Sliders - Danh sách tất cả Slider
    /// Use Case: Admin xem danh sách Slider
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var sliders = await _context.Sliders
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();

        return View(new SliderListViewModel { Sliders = sliders });
    }

    /// <summary>
    /// GET: /Admin/Sliders/Create - Form tạo Slider mới
    /// Use Case: Admin tạo Slider
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        return View(new SliderFormViewModel());
    }

    /// <summary>
    /// POST: /Admin/Sliders/Create - Lưu Slider mới vào DB
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SliderFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var slider = new Slider
        {
            Title = model.Title,
            Description = model.Description,
            ImageUrl = model.ImageUrl,
            LinkUrl = model.LinkUrl,
            DisplayOrder = model.DisplayOrder,
            Status = model.Status,
            CreatedAt = DateTime.UtcNow
        };

        _context.Sliders.Add(slider);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Tạo Slider thành công!";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// GET: /Admin/Sliders/Edit/{id} - Form chỉnh sửa Slider
    /// Use Case: Admin chỉnh sửa Slider
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var slider = await _context.Sliders.FindAsync(id);
        if (slider is null) return NotFound();

        var model = new SliderFormViewModel
        {
            Id = slider.Id,
            Title = slider.Title,
            Description = slider.Description,
            ImageUrl = slider.ImageUrl,
            LinkUrl = slider.LinkUrl,
            DisplayOrder = slider.DisplayOrder,
            Status = slider.Status
        };

        return View(model);
    }

    /// <summary>
    /// POST: /Admin/Sliders/Edit/{id} - Cập nhật Slider vào DB
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SliderFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var slider = await _context.Sliders.FindAsync(id);
        if (slider is null) return NotFound();

        slider.Title = model.Title;
        slider.Description = model.Description;
        slider.ImageUrl = model.ImageUrl;
        slider.LinkUrl = model.LinkUrl;
        slider.DisplayOrder = model.DisplayOrder;
        slider.Status = model.Status;
        slider.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Cập nhật Slider thành công!";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// POST: /Admin/Sliders/ToggleStatus/{id} - Bật/Tắt Slider
    /// Use Case: Admin thay đổi trạng thái Slider
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var slider = await _context.Sliders.FindAsync(id);
        if (slider is null) return NotFound();

        slider.Status = slider.Status == SliderStatus.Active
            ? SliderStatus.Inactive
            : SliderStatus.Active;

        slider.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
