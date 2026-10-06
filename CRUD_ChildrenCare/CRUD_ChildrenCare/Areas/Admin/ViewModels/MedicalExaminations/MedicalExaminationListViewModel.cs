using CRUD_ChildrenCare.Models;

namespace CRUD_ChildrenCare.Areas.Admin.ViewModels.MedicalExaminations;

public sealed class MedicalExaminationListViewModel
{
    public IReadOnlyList<MedicalExamination> Examinations { get; set; } = [];
    public IReadOnlyList<Service> Services { get; set; } = [];
    public int? ServiceId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? MedicineName { get; set; }
    public string? SearchQuery { get; set; }
    public int PageIndex { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalItems { get; set; }
    public bool CanCreate { get; set; }
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
}
