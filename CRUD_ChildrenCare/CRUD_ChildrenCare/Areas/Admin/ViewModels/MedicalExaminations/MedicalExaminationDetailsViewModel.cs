using CRUD_ChildrenCare.Models;

namespace CRUD_ChildrenCare.Areas.Admin.ViewModels.MedicalExaminations;

public sealed class MedicalExaminationDetailsViewModel
{
    public MedicalExamination Examination { get; set; } = null!;
    public bool CanCreate { get; set; }
}
