using System.ComponentModel.DataAnnotations;

namespace CRUD_ChildrenCare.Models;

/// <summary>
/// Model đại diện cho bài viết (Post) trong hệ thống
/// [PART 2 - MODEL LAYER]
/// </summary>
public class Post
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string Summary { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? ThumbnailUrl { get; set; }

    public PostStatus Status { get; set; } = PostStatus.Draft;

    public bool IsFeatured { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Foreign Key → User (Author)
    public string? AuthorId { get; set; }
    public User? Author { get; set; }
}
