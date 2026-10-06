using CRUD_ChildrenCare.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRUD_ChildrenCare.Models;

public sealed class Feedback
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    public Gender Gender { get; set; }

    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(10)]
    public string Mobile { get; set; } = string.Empty;

    public int? ServiceId { get; set; }
    [ForeignKey("ServiceId")]
    public Service? Service { get; set; }

    [Range(1, 5)]
    public int RatedStar { get; set; }

    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;

    public FeedbackStatus Status { get; set; } = FeedbackStatus.Published;

    public DateTime CreatedDate { get; set; }

    public ICollection<FeedbackImage> FeedbackImages { get; set; } = new List<FeedbackImage>();
}
