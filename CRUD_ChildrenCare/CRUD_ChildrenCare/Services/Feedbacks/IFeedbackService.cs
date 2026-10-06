using CRUD_ChildrenCare.Models;
using CRUD_ChildrenCare.Models.Enums;
using CRUD_ChildrenCare.ViewModels.Feedback;

namespace CRUD_ChildrenCare.Services.Feedbacks;

public interface IFeedbackService
{
    Task<Feedback> CreateFeedbackAsync(CreateFeedbackViewModel model, CancellationToken ct = default);
    Task<(IReadOnlyList<Feedback> Feedbacks, int TotalItems, int TotalPages)> GetFeedbacksAsync(
        string? search, FeedbackStatus? status, int? serviceId, int? ratedStar, string? sort, int pageIndex, int pageSize, CancellationToken ct = default);
    Task<Feedback?> GetFeedbackByIdAsync(int id, CancellationToken ct = default);
    Task<bool> ToggleStatusAsync(int id, CancellationToken ct = default);
    Task<bool> UpdateStatusAsync(int id, FeedbackStatus status, CancellationToken ct = default);
    Task<IReadOnlyList<Feedback>> GetPublishedFeedbacksForServiceAsync(int? serviceId, int limit = 10, CancellationToken ct = default);
}
