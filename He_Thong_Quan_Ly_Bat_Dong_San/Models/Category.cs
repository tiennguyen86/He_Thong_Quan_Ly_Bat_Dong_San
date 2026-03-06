using System.ComponentModel.DataAnnotations;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Models;

public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên loại BĐS")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên phải từ 3 đến 100 ký tự")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Navigation property
    public ICollection<Property>? Properties { get; set; }
}