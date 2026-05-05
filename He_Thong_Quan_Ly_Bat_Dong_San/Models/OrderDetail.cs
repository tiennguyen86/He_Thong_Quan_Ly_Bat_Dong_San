using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Models;

/// <summary>
/// Model đại diện cho từng bất động sản cụ thể trong một đơn lịch hẹn
/// Mối quan hệ Many-to-Many giữa Order và Property thông qua bảng này
/// </summary>
public class OrderDetail
{
    /// <summary>
    /// Mã chi tiết (ID)
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Khóa ngoại: Liên kết tới đơn lịch hẹn
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Navigation property: Đơn lịch hẹn chứa chi tiết này
    /// </summary>
    [ForeignKey("OrderId")]
    public Order? Order { get; set; }

    /// <summary>
    /// Khóa ngoại: Liên kết tới bất động sản mà khách muốn xem
    /// </summary>
    public int PropertyId { get; set; }

    /// <summary>
    /// Navigation property: Bất động sản cụ thể trong đơn lịch hẹn này
    /// </summary>
    [ForeignKey("PropertyId")]
    public Property? Property { get; set; }

    /// <summary>
    /// Giá tại thời điểm đặt lịch
    /// Lưu lại giá cố định để phòng trường hợp sau này BĐS tăng/giảm giá
    /// Đảm bảo tính toàn vẹn của dữ liệu lịch sử
    /// </summary>
    public decimal Price { get; set; } 
}