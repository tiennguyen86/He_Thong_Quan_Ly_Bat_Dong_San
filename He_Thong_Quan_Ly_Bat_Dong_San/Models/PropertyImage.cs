using System.ComponentModel.DataAnnotations;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Models
{
    public class PropertyImage
    {
        [Key]
        public int Id { get; set; }
        
        public string ImageUrl { get; set; } // Lưu đường dẫn ảnh

        // Khóa ngoại liên kết với bảng Property
        public int PropertyId { get; set; }
        public Property Property { get; set; }
    }
}