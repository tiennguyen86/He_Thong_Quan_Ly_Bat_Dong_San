using System.ComponentModel.DataAnnotations;

namespace He_Thong_Quan_Ly_Bat_Dong_San.ViewModels;

/// <summary>
/// ViewModel dùng để nhận dữ liệu từ form đăng nhập
/// Chứa các trường xác thực để đảm bảo dữ liệu hợp lệ
/// </summary>
public class LoginVM
{
    /// <summary>
    /// Email/Username của người dùng
    /// Bắt buộc nhập và phải có định dạng email hợp lệ
    /// </summary>
    [Required(ErrorMessage = "Vui lòng nhập Email")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu của người dùng
    /// Bắt buộc nhập
    /// </summary>
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Tùy chọn "Ghi nhớ đăng nhập"
    /// Nếu true sẽ tạo persistent cookie để người dùng không cần đăng nhập lại
    /// </summary>
    public bool RememberMe { get; set; }
}