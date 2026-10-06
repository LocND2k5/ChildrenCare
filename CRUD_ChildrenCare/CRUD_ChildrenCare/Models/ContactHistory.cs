using CRUD_ChildrenCare.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRUD_ChildrenCare.Models;

public sealed class ContactHistory
{
    public int Id { get; set; }

    public int ContactId { get; set; }
    public Contact Contact { get; set; } = null!;

    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    public Gender Gender { get; set; }

    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(10)]
    public string Mobile { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Address { get; set; }

    public int? UpdatedById { get; set; }
    [ForeignKey("UpdatedById")]
    public User? UpdatedBy { get; set; }

    public DateTime UpdatedDate { get; set; }
}
