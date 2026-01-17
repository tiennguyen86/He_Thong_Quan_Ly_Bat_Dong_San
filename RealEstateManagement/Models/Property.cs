using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RealEstateManagement.Models;


public class Property
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Tiêu đề tin")]
    [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
    [MaxLength(100, ErrorMessage = "Tiêu đề không quá 100 ký tự")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Mô tả chi tiết")]
    public string? Description { get; set; }

    [Display(Name = "Giá (VNĐ)")]
    [Required(ErrorMessage = "Vui lòng nhập giá")]
    [Column(TypeName = "decimal(18,0)")] // Lưu số tiền lớn
    public decimal Price { get; set; }

    [Display(Name = "Diện tích (m2)")]
    [Required]
    public double Area { get; set; }

    [Display(Name = "Địa chỉ")]
    [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
    public string Address { get; set; } = string.Empty;

    [Display(Name = "Loại hình")]
    public string Type { get; set; } = "Bán"; // Mặc định là Bán

    // Tự động lưu thời gian tạo (Điểm cộng: Audit data)
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    // Đường dẫn ảnh (sẽ làm tính năng upload sau)
    public string? ImagePath { get; set; }
}