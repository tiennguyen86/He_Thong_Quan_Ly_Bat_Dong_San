using System.ComponentModel.DataAnnotations;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Models;

/// <summary>
/// Model đại diện cho một loại danh mục bất động sản
/// Ví dụ: Nhà ở, Đất nền, Chung cư, Nhà cho thuê...
/// </summary>
public class Category
{
    /// <summary>
    /// Mã danh mục (ID)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tên danh mục của bất động sản
    /// Bắt buộc nhập, độ dài 3-100 ký tự
    /// </summary>
    [Required(ErrorMessage = "Vui lòng nhập tên loại BĐS")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên phải từ 3 đến 100 ký tự")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả chi tiết về danh mục này (tùy chọn)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Navigation property: Danh sách tất cả bất động sản thuộc danh mục này
    /// Mối quan hệ 1-Nhiều: Một danh mục có nhiều bất động sản
    /// </summary>
    public ICollection<Property>? Properties { get; set; }
}