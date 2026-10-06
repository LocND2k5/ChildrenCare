using CRUD_ChildrenCare.Models;

namespace CRUD_ChildrenCare.Areas.Admin.ViewModels.Feedbacks;

public sealed class FeedbackDetailsAdminViewModel
{
    public Feedback Feedback { get; set; } = null!;
    public bool CanManage { get; set; }
}
