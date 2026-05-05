using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Models;

/// <summary>
/// Model đại diện cho một bất động sản trong hệ thống
/// Bao gồm thông tin chi tiết về nhà đất, giá cả, vị trí, hình ảnh
/// </summary>
public class Property
{
    /// <summary>
    /// Mã bất động sản (ID)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tiêu đề/Tên gọi bất động sản
    /// Bắt buộc nhập, tối đa 200 ký tự
    /// </summary>
    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    [StringLength(200, ErrorMessage = "Tiêu đề tối đa 200 ký tự")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Giá bán bất động sản (tính bằng VND)
    /// Bắt buộc nhập, giá trị > 0
    /// </summary>
    [Required(ErrorMessage = "Vui lòng nhập giá")]
    [Range(typeof(decimal), "0", "999999999999", ErrorMessage = "Giá phải lớn hơn 0")]
    public decimal Price { get; set; }

    /// <summary>
    /// Diện tích bất động sản (tính bằng m²)
    /// Bắt buộc nhập, giá trị > 0.1
    /// </summary>
    [Required(ErrorMessage = "Vui lòng nhập diện tích")]
    [Range(0.1, double.MaxValue, ErrorMessage = "Diện tích phải lớn hơn 0")]
    public double Area { get; set; }

    /// <summary>
    /// Địa chỉ chi tiết của bất động sản
    /// Bắt buộc nhập
    /// </summary>
    [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
    public string Address { get; set; } = string.Empty;
    
    /// <summary>
    /// Mô tả chi tiết về bất động sản (tiện ích, tình trạng, v.v...)
    /// Bắt buộc nhập
    /// </summary>
    [Required(ErrorMessage = "Vui lòng nhập mô tả")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Đường dẫn ảnh đại diện của bất động sản
    /// Được lưu vào database và phục vụ từ wwwroot/
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Khóa ngoại: Liên kết tới danh mục
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Navigation property: Danh mục mà bất động sản này thuộc về
    /// </summary>
    [ForeignKey("CategoryId")]
    public Category? Category { get; set; }

    /// <summary>
    /// File ảnh được upload từ form
    /// Không được lưu vào database, chỉ dùng để nhận dữ liệu từ giao diện
    /// </summary>
    [NotMapped]
    [Display(Name = "Hình ảnh BĐS")]
    public IFormFile? ImageUpload { get; set; }
    
    /// <summary>
    /// Navigation property: Danh sách tất cả ảnh trong gallery
    /// Mối quan hệ 1-Nhiều: Một BĐS có nhiều ảnh chi tiết
    /// </summary>
    public ICollection<PropertyImage>? PropertyImages { get; set; }

    /// <summary>
    /// Danh sách các file ảnh gallery được upload từ form
    /// Không được lưu vào database, chỉ dùng để nhận dữ liệu từ giao diện
    /// </summary>
    [NotMapped]
    public List<IFormFile>? GalleryUploads { get; set; }
}