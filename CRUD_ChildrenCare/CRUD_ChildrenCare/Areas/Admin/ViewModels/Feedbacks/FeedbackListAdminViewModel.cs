using CRUD_ChildrenCare.Models;
using CRUD_ChildrenCare.Models.Enums;

namespace CRUD_ChildrenCare.Areas.Admin.ViewModels.Feedbacks;

public sealed class FeedbackListAdminViewModel
{
    public IReadOnlyList<Feedback> Feedbacks { get; set; } = [];
    public IReadOnlyList<Service> Services { get; set; } = [];
    public string? SearchQuery { get; set; }
    public FeedbackStatus? Status { get; set; }
    public int? ServiceId { get; set; }
    public int? RatedStar { get; set; }
    public string? Sort { get; set; }
    public int PageIndex { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalItems { get; set; }
    public bool CanManage { get; set; }
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
}
