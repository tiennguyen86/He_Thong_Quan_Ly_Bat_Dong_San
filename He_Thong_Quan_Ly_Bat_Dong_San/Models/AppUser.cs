using Microsoft.AspNetCore.Identity;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Models;

/// <summary>
/// Model mở rộng IdentityUser để lưu thêm thông tin người dùng tùy chỉnh
/// </summary>
public class AppUser : IdentityUser
{
    /// <summary>
    /// Họ và tên của người dùng
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Trạng thái tài khoản: true = Hoạt động, false = Bị khóa/Xóa
    /// Dùng để quản lý quyền truy cập mà không xóa tài khoản khỏi database
    /// </summary>
    public bool IsActive { get; set; } = true;
}