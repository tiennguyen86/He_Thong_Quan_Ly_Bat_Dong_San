using System.ComponentModel.DataAnnotations;

namespace He_Thong_Quan_Ly_Bat_Dong_San.ViewModels;

public class LoginVM
{
    [Required(ErrorMessage = "Vui lòng nhập Email")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } // Chức năng "Ghi nhớ đăng nhập"
}