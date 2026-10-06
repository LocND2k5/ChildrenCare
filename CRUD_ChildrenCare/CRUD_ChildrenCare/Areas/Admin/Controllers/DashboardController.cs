using CRUD_ChildrenCare.Data;
using CRUD_ChildrenCare.Services.Dashboards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRUD_ChildrenCare.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SystemData.RoleValues.Admin + "," + SystemData.RoleValues.Manager)]
public sealed class DashboardController(IDashboardService dashboardService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
    {
        var isManager = User.IsInRole(SystemData.RoleValues.Manager) && !User.IsInRole(SystemData.RoleValues.Admin);
        var model = await dashboardService.GetDashboardDataAsync(
            fromDate, toDate, isManager, HttpContext.RequestAborted);

        return View(model);
    }
}
