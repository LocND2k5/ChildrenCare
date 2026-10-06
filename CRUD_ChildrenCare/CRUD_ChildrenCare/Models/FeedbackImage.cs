using System.ComponentModel.DataAnnotations;

namespace CRUD_ChildrenCare.Models;

public sealed class FeedbackImage
{
    public int Id { get; set; }

    public int FeedbackId { get; set; }
    public Feedback Feedback { get; set; } = null!;

    [MaxLength(255)]
    public string ImageUrl { get; set; } = string.Empty;
}
