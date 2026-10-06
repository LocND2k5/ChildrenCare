using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRUD_ChildrenCare.Models;

public sealed class PrescriptionItem
{
    public int Id { get; set; }

    public int PrescriptionId { get; set; }
    [ForeignKey("PrescriptionId")]
    public Prescription Prescription { get; set; } = null!;

    [MaxLength(150)]
    public string MedicineName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Dosage { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    [MaxLength(255)]
    public string? Usage { get; set; }
}
