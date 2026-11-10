using System.ComponentModel.DataAnnotations;

namespace CRUD_ChildrenCare.Models;

/// <summary>
/// Model đại diện cho Slider ảnh trên trang chủ
/// [PART 2 - MODEL LAYER]
/// </summary>
public class Slider
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required, MaxLength(300)]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? LinkUrl { get; set; }

    public int DisplayOrder { get; set; } = 0;

    public SliderStatus Status { get; set; } = SliderStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
