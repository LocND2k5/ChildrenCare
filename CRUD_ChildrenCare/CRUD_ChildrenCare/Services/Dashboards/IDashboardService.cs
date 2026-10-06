using CRUD_ChildrenCare.Areas.Admin.ViewModels.Dashboard;

namespace CRUD_ChildrenCare.Services.Dashboards;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardDataAsync(DateTime? fromDate, DateTime? toDate, bool isManager, CancellationToken ct = default);
}
