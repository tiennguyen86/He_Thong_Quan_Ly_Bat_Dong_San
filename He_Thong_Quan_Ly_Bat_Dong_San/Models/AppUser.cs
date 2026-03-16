using Microsoft.AspNetCore.Identity;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Models;

public class AppUser : IdentityUser
{
    // Bạn có thể thêm các trường tùy ý ở đây
    public string? FullName { get; set; }
    // THÊM DÒNG NÀY: 1 (true) là Hoạt động, 0 (false) là Đã xóa/Khóa
    public bool IsActive { get; set; } = true;
}