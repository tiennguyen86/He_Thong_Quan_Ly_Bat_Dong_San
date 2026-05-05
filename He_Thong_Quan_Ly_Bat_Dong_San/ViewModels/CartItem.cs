namespace He_Thong_Quan_Ly_Bat_Dong_San.Models;

/// <summary>
/// ViewModel đại diện cho một mục trong giỏ hàng
/// Dùng để lưu trữ tạm thời thông tin BĐS mà khách chọn
/// Được serialized dưới dạng JSON để lưu vào Session
/// </summary>
public class CartItem
{
    /// <summary>
    /// Mã bất động sản (ID)
    /// </summary>
    public int PropertyId { get; set; }

    /// <summary>
    /// Tên/Tiêu đề bất động sản
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Giá bán bất động sản lúc được thêm vào giỏ
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Đường dẫn ảnh đại diện của bất động sản
    /// Dùng để hiển thị trong giỏ hàng
    /// Mặc định sẽ là ảnh mặc định nếu BĐS không có ảnh
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;
}