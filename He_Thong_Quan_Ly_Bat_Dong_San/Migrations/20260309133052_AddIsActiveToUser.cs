using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace He_Thong_Quan_Ly_Bat_Dong_San.Migrations
{
    /// <summary>
    /// Migration lần 4: Thêm trạng thái hoạt động cho user
    /// Ngày: 09/03/2026
    /// 
    /// Thêm cột IsActive vào bảng AspNetUsers:
    /// - true = Tài khoản hoạt động (được phép đăng nhập)
    /// - false = Tài khoản bị khóa/xóa (không được đăng nhập)
    /// 
    /// Lợi ích: 
    /// - Quản trị viên có thể khóa tài khoản mà không xóa khỏi DB
    /// - Giữ được lịch sử dữ liệu (audit trail)
    /// - Hỗ trợ soft delete
    /// 
    /// Mặc định: IsActive = false khi migration chạy
    /// (Được update = true ở App.cs khi tạo user mới)
    /// </summary>
    /// <inheritdoc />
    public partial class AddIsActiveToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUsers");
        }
    }
}
