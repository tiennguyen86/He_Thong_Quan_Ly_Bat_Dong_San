using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Models;

public class Property
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    [StringLength(200, ErrorMessage = "Tiêu đề tối đa 200 ký tự")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập giá")]
    [Range(typeof(decimal), "0", "999999999999", ErrorMessage = "Giá phải lớn hơn 0")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập diện tích")]
    [Range(0.1, double.MaxValue, ErrorMessage = "Diện tích phải lớn hơn 0")]
    public double Area { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
    public string Address { get; set; } = string.Empty;

    // Link ảnh trong database
    public string? ImageUrl { get; set; }

    // Foreign key
    public int CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public Category? Category { get; set; }

    // Upload file (không lưu DB)
    [NotMapped]
    [Display(Name = "Hình ảnh BĐS")]
    public IFormFile? ImageUpload { get; set; }
}