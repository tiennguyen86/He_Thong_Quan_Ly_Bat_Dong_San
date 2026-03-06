using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Models;

public class Order
{
    [Key]
    public int Id { get; set; }

    // Liên kết với tài khoản người dùng đã đăng nhập (Để biết ai đặt lịch)
    public string? AppUserId { get; set; }
    [ForeignKey("AppUserId")]
    public AppUser? AppUser { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên khách hàng")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; } = DateTime.Now;
    
    public string? Notes { get; set; }

    // Trạng thái đơn lịch hẹn (Ví dụ: Chờ xác nhận, Đã liên hệ, Hủy...)
    public string Status { get; set; } = "Chờ xác nhận"; 

    // Móc nối sang bảng Chi tiết (OrderDetail)
    public ICollection<OrderDetail>? OrderDetails { get; set; }
}