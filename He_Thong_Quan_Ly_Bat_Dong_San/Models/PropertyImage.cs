using System.ComponentModel.DataAnnotations;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Models
{
    /// <summary>
    /// Model đại diện cho một hình ảnh trong danh sách gallery của bất động sản
    /// Cho phép mỗi BĐS có nhiều ảnh chi tiết
    /// </summary>
    public class PropertyImage
    {
        /// <summary>
        /// Mã hình ảnh (ID)
        /// </summary>
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// Đường dẫn tới file ảnh
        /// Được lưu vào database và phục vụ từ wwwroot/
        /// Ví dụ: "/uploads/gallery/abc123_photo.jpg"
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Khóa ngoại: Liên kết tới bất động sản sở hữu ảnh này
        /// Mối quan hệ Many-to-One: Nhiều ảnh thuộc về một BĐS
        /// </summary>
        public int PropertyId { get; set; }

        /// <summary>
        /// Navigation property: Bất động sản mà ảnh này thuộc về
        /// </summary>
        public Property Property { get; set; }
    }
}