using He_Thong_Quan_Ly_Bat_Dong_San.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Data;

/// <summary>
/// DbContext chính của ứng dụng
/// Kế thừa từ IdentityDbContext để hỗ trợ tính năng xác thực/phân quyền
/// Quản lý tất cả các entity và mối quan hệ trong database
/// </summary>
public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    /// <summary>
    /// Constructor: Nhận cấu hình database options từ Dependency Injection
    /// </summary>
    /// <param name="options">Cấu hình database connection string và provider</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Bảng Danh mục bất động sản
    /// Ví dụ: Nhà ở, Đất nền, Chung cư, etc.
    /// </summary>
    public DbSet<Category> Categories { get; set; }

    /// <summary>
    /// Bảng Bất động sản chính
    /// Chứa thông tin chi tiết về từng căn nhà/lô đất
    /// </summary>
    public DbSet<Property> Properties { get; set; }

    /// <summary>
    /// Bảng Đơn lịch hẹn xem bất động sản
    /// Lưu trữ các request từ khách hàng
    /// </summary>
    public DbSet<Order> Orders { get; set; }

    /// <summary>
    /// Bảng Hình ảnh bất động sản (Gallery)
    /// Lưu trữ các ảnh chi tiết của từng BĐS
    /// </summary>
    public DbSet<PropertyImage> PropertyImages { get; set; }

    /// <summary>
    /// Bảng Chi tiết đơn lịch hẹn
    /// Ghi lại từng BĐS cụ thể trong một đơn lịch hẹn
    /// </summary>
    public DbSet<OrderDetail> OrderDetails { get; set; }
}