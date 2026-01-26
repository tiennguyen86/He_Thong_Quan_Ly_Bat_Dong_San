using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateApp.Models;

public class BatDongSan
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tiêu đề tin")]
    [StringLength(100, ErrorMessage = "Tiêu đề không quá 100 ký tự")]
    public string TieuDe { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn 0")]
    public decimal Gia { get; set; }

    public string? HinhAnh { get; set; } // Lưu tên file ảnh (VD: nha-dep.jpg)

    public string? MoTa { get; set; }
    
    public string? DiaChi { get; set; }

    public bool IsHot { get; set; } // Sản phẩm nổi bật

    // Khóa ngoại sang bảng LoaiBatDongSan
    public int LoaiBatDongSanId { get; set; }
    [ForeignKey("LoaiBatDongSanId")]
    public LoaiBatDongSan? LoaiBatDongSan { get; set; }
}