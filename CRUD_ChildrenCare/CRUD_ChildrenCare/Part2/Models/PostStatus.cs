namespace CRUD_ChildrenCare.Models;

/// <summary>
/// Enum trạng thái bài viết
/// [PART 2 - MODEL LAYER]
/// </summary>
public enum PostStatus
{
    Draft = 0,      // Bản nháp - chưa hiển thị công khai
    Published = 1,  // Đã xuất bản - hiển thị công khai
    Archived = 2    // Lưu trữ - ẩn khỏi trang web
}
