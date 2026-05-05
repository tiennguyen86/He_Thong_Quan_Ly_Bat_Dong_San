using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace He_Thong_Quan_Ly_Bat_Dong_San.Migrations
{
    /// <summary>
    /// Migration lần 6: Thêm cột mô tả cho BĐS
    /// Ngày: 09/03/2026
    /// 
    /// Thêm cột Description vào bảng Properties:
    /// - type: nvarchar(max) → Hỗ trợ mô tả dài
    /// - Là bắt buộc (nullable: false)
    /// - defaultValue: "" → Set giá trị rỗng cho record cũ
    /// 
    /// Mục đích:
    /// - Lưu mô tả chi tiết về BĐS
    /// - VD: Tiện ích, tình trạng, pháp lý,...
    /// 
    /// Lợi ích:
    /// - Tăng thông tin chi tiết trên trang Details
    /// - Cải thiện SEO & trải nghiệm khách hàng
    /// </summary>
    /// <inheritdoc />
    public partial class AddDescriptionColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Properties");
        }
    }
}
