using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Models;

public class OrderDetail
{
    [Key]
    public int Id { get; set; }

    // Nối với bảng Order
    public int OrderId { get; set; }
    [ForeignKey("OrderId")]
    public Order? Order { get; set; }

    // Nối với bảng Sản phẩm (BĐS khách muốn xem)
    public int PropertyId { get; set; }
    [ForeignKey("PropertyId")]
    public Property? Property { get; set; }

    // Lưu lại giá tiền tại thời điểm đặt (phòng trường hợp sau này BĐS tăng giá)
    public decimal Price { get; set; } 
}