using CRUD_ChildrenCare.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace CRUD_ChildrenCare.Models;

public sealed class Contact
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    public Gender Gender { get; set; }

    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(10)]
    public string Mobile { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Address { get; set; }

    public ContactStatus Status { get; set; } = ContactStatus.Contact;

    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    public ICollection<ContactHistory> Histories { get; set; } = new List<ContactHistory>();
}
