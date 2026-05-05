using System.ComponentModel.DataAnnotations;

namespace He_Thong_Quan_Ly_Bat_Dong_San.ViewModels;

/// <summary>
/// ViewModel dùng để nhận dữ liệu từ form đăng ký tài khoản
/// Chứa các trường xác thực để đảm bảo dữ liệu hợp lệ
/// </summary>
public class RegisterVM
{
    /// <summary>
    /// Họ và tên người dùng
    /// Bắt buộc nhập
    /// </summary>
    [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Email của người dùng
    /// Dùng làm tên đăng nhập (Username)
    /// Bắt buộc nhập và phải có định dạng email hợp lệ
    /// </summary>
    [Required(ErrorMessage = "Vui lòng nhập Email")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu người dùng
    /// Bắt buộc nhập
    /// </summary>
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Xác nhận mật khẩu
    /// Bắt buộc nhập và phải trùng với Password
    /// </summary>
    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    public string ConfirmPassword { get; set; } = string.Empty;
}