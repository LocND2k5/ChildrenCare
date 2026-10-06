using System.Security.Claims;
using CRUD_ChildrenCare.Areas.Admin.ViewModels.Customers;
using CRUD_ChildrenCare.Data;
using CRUD_ChildrenCare.Models.Enums;
using CRUD_ChildrenCare.Services.Contacts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRUD_ChildrenCare.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SystemData.RoleValues.Manager + "," + SystemData.RoleValues.Admin + "," + SystemData.RoleValues.Doctor + "," + SystemData.RoleValues.Nurse)]
public sealed class CustomersController(
    IContactService contactService,
    ApplicationDbContext dbContext) : Controller
{
    private const int PageSize = 10;

    private bool IsManagerOrAdmin =>
        User.IsInRole(SystemData.RoleValues.Manager) || User.IsInRole(SystemData.RoleValues.Admin);

    private int? CurrentUserId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    [HttpGet]
    public async Task<IActionResult> Index(string? search, ContactStatus? status, string? sort, int page = 1)
    {
        var (contacts, totalItems, totalPages) = await contactService.GetContactsAsync(
            search, status, sort, page, PageSize, HttpContext.RequestAborted);

        return View(new CustomerListViewModel
        {
            Customers = contacts,
            SearchQuery = search,
            Status = status,
            Sort = sort,
            PageIndex = page,
            TotalPages = totalPages,
            TotalItems = totalItems,
            CanManage = IsManagerOrAdmin
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var customer = await contactService.GetContactByIdAsync(id, HttpContext.RequestAborted);
        if (customer == null)
        {
            return NotFound();
        }

        var reservations = await dbContext.Reservations
            .Include(r => r.ReservationItems).ThenInclude(i => i.Service)
            .Where(r => r.ReceiverEmail.ToLower() == customer.Email.ToLower())
            .OrderByDescending(r => r.ReservedDate)
            .Take(10)
            .ToListAsync(HttpContext.RequestAborted);

        return View(new CustomerDetailsViewModel
        {
            Customer = customer,
            Histories = customer.Histories.OrderByDescending(h => h.UpdatedDate).ToList(),
            Reservations = reservations,
            CanManage = IsManagerOrAdmin
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (!IsManagerOrAdmin)
        {
            return Forbid();
        }

        return View(new CreateCustomerViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCustomerViewModel model)
    {
        if (!IsManagerOrAdmin)
        {
            return Forbid();
        }

        if (await contactService.EmailExistsAsync(model.Email, null, HttpContext.RequestAborted))
        {
            ModelState.AddModelError(nameof(model.Email), "Email này đã tồn tại trong danh sách khách hàng.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var success = await contactService.CreateCustomerAsync(model, CurrentUserId, HttpContext.RequestAborted);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Không thể tạo khách hàng.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Thêm mới thông tin khách hàng thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!IsManagerOrAdmin)
        {
            return Forbid();
        }

        var customer = await contactService.GetContactByIdAsync(id, HttpContext.RequestAborted);
        if (customer == null)
        {
            return NotFound();
        }

        return View(new EditCustomerViewModel
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Gender = customer.Gender,
            Email = customer.Email,
            Mobile = customer.Mobile,
            Address = customer.Address,
            Status = customer.Status
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditCustomerViewModel model)
    {
        if (!IsManagerOrAdmin)
        {
            return Forbid();
        }

        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var success = await contactService.UpdateCustomerAsync(model, CurrentUserId, HttpContext.RequestAborted);
        if (!success)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Cập nhật thông tin khách hàng thành công.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
