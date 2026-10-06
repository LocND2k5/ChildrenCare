using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRUD_ChildrenCare.Models;

public sealed class MedicalExamination
{
    public int Id { get; set; }

    public int ReservationId { get; set; }
    [ForeignKey("ReservationId")]
    public Reservation Reservation { get; set; } = null!;

    public DateTime ExaminationDate { get; set; }

    public int DoctorId { get; set; }
    [ForeignKey("DoctorId")]
    public User Doctor { get; set; } = null!;

    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
