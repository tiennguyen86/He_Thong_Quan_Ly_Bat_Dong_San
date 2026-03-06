using Microsoft.AspNetCore.Identity;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Models;

public class AppUser : IdentityUser
{
    // Bạn có thể thêm các trường tùy ý ở đây
    public string? FullName { get; set; }
}