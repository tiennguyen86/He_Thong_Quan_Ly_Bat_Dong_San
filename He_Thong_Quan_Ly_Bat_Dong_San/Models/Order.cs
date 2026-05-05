using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Models;

/// <summary>
/// Model đại diện cho một đơn lịch hẹn xem bất động sản
/// Chứa thông tin khách hàng và trạng thái đơn
/// </summary>
public class Order
{
    /// <summary>
    /// Mã đơn hàng (ID)
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Khóa ngoại: Liên kết tới tài khoản người dùng đã đăng nhập
    /// Để biết ai là người đặt lịch hẹn (có thể null nếu khách chưa đăng nhập)
    /// </summary>
    public string? AppUserId { get; set; }

    /// <summary>
    /// Navigation property: Thông tin người dùng đã đăng nhập
    /// </summary>
    [ForeignKey("AppUserId")]
    public AppUser? AppUser { get; set; }

    /// <summary>
    /// Tên khách hàng muốn xem bất động sản
    /// Bắt buộc nhập
    /// </summary>
    [Required(ErrorMessage = "Vui lòng nhập tên khách hàng")]
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Số điện thoại liên hệ của khách hàng
    /// Bắt buộc nhập, phải là số Việt Nam hợp lệ (10 chữ số, bắt đầu bằng 03/05/07/08/09)
    /// </summary>
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Ngày giờ đặt lịch hẹn
    /// Mặc định là thời điểm hiện tại
    /// </summary>
    public DateTime OrderDate { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Ghi chú thêm từ khách hàng (tùy chọn)
    /// Ví dụ: "Thích xem vào cuối tuần"
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Trạng thái của đơn lịch hẹn
    /// Các giá trị có thể: "Chờ xác nhận", "Đã liên hệ", "Đã xem", "Hủy"
    /// Mặc định: "Chờ xác nhận"
    /// </summary>
    public string Status { get; set; } = "Chờ xác nhận"; 

    /// <summary>
    /// Navigation property: Danh sách chi tiết các bất động sản trong đơn lịch hẹn này
    /// Mối quan hệ 1-Nhiều: Một đơn có nhiều chi tiết (nhiều BĐS)
    /// </summary>
    public ICollection<OrderDetail>? OrderDetails { get; set; }
}