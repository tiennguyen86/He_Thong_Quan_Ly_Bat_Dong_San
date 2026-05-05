using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace He_Thong_Quan_Ly_Bat_Dong_San.Migrations
{
    /// <summary>
    /// Migration lần 5: Tạo bảng gallery ảnh cho BĐS
    /// Ngày: 09/03/2026
    /// 
    /// Tạo bảng PropertyImages:
    /// - Cho phép mỗi BĐS có nhiều ảnh chi tiết (gallery)
    /// - Id: Mã ảnh (Primary Key)
    /// - ImageUrl: Đường dẫn ảnh (lưu vào wwwroot/uploads/gallery/)
    /// - PropertyId (FK) → Property
    /// 
    /// Mối quan hệ:
    /// - Property 1 -* PropertyImages (1 BĐS có nhiều ảnh)
    /// - Cascade Delete: Xóa BĐS → xóa tất cả ảnh liên quan
    /// 
    /// Lợi ích:
    /// - Hiển thị đa ảnh cho mỗi BĐS
    /// - Tạo album ảnh chi tiết
    /// </summary>
    /// <inheritdoc />
    public partial class AddPropertyGallery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PropertyImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyImages_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyImages_PropertyId",
                table: "PropertyImages",
                column: "PropertyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropertyImages");
        }
    }
}
