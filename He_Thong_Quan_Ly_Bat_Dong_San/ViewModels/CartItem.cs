namespace He_Thong_Quan_Ly_Bat_Dong_San.Models;

public class CartItem
{
    public int PropertyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}