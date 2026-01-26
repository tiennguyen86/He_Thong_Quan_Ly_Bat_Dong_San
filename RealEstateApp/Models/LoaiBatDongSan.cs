using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Models;

public class LoaiBatDongSan
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên loại không được để trống")]
    [Display(Name = "Tên Loại BĐS")]
    public string TenLoai { get; set; } // Ví dụ: Căn hộ, Nhà phố
    
    // Quan hệ 1-nhiều: Một loại có nhiều bất động sản
    public ICollection<BatDongSan>? BatDongSans { get; set; }
}