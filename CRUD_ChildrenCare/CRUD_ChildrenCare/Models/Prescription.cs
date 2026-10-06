using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRUD_ChildrenCare.Models;

public sealed class Prescription
{
    public int Id { get; set; }

    public int ExaminationId { get; set; }
    [ForeignKey("ExaminationId")]
    public MedicalExamination MedicalExamination { get; set; } = null!;

    [MaxLength(500)]
    public string Diagnosis { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Note { get; set; }

    public DateTime CreatedDate { get; set; }

    public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
}
