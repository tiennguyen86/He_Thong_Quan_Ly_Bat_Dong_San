using Microsoft.AspNetCore.Mvc;
using He_Thong_Quan_Ly_Bat_Dong_San.Data;
using He_Thong_Quan_Ly_Bat_Dong_San.Models;
using He_Thong_Quan_Ly_Bat_Dong_San.Helpers; // Dùng cái đồ nghề vừa tạo
using System.Security.Claims; // Dùng để lấy ID người dùng đăng nhập

namespace He_Thong_Quan_Ly_Bat_Dong_San.Controllers;

public class CartController : Controller
{
    private readonly ApplicationDbContext _context;

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Biến hằng số này là cái "chìa khóa" để mở tủ Session
    private const string CART_KEY = "MyCart";

    public IActionResult Index()
    {
        // Lấy danh sách từ Session ra
        var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
        return View(cart);
    }

    public IActionResult AddToCart(int id)
    {
        // 1. Tìm BĐS dưới database
        var property = _context.Properties.Find(id);
        if (property == null) return NotFound();

        // 2. Mở tủ Session lấy cái giỏ hàng hiện tại ra
        var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();

        // 3. Kiểm tra xem nhà này đã có trong giỏ chưa (không cho thêm 1 căn 2 lần)
        var item = cart.FirstOrDefault(c => c.PropertyId == id);
        if (item == null)
        {
            // Nếu chưa có thì ném nó vào giỏ
            cart.Add(new CartItem
            {
                PropertyId = property.Id,
                Title = property.Title,
                Price = property.Price,
                ImageUrl = property.ImageUrl ?? "/images/default.jpg"
            });
        }

        // 4. Cất cái giỏ hàng (đã có thêm đồ mới) vào tủ Session lại
        HttpContext.Session.Set(CART_KEY, cart);

        // 5. Đá người dùng về trang Giỏ hàng để xem
        return RedirectToAction(nameof(Index));
    }
    public IActionResult RemoveFromCart(int id)
    {
        var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
        
        // Tìm căn nhà khách muốn xóa và gỡ nó ra khỏi danh sách
        var itemToRemove = cart.FirstOrDefault(c => c.PropertyId == id);
        if (itemToRemove != null)
        {
            cart.Remove(itemToRemove);
            HttpContext.Session.Set(CART_KEY, cart); // Cất giỏ hàng mới vào lại Session
        }

        return RedirectToAction(nameof(Index));
    }
    // 1. Hiển thị Form điền thông tin
    [HttpGet]
    public IActionResult Checkout()
    {
        var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
        if (!cart.Any()) 
        {
            return RedirectToAction(nameof(Index)); // Giỏ trống thì đuổi về
        }
        return View(new Order()); // Truyền cái vỏ Order rỗng sang để khách điền
    }

    // 2. Xử lý khi khách bấm nút "Xác nhận gửi"
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(Order order)
    {
        var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
        if (!cart.Any()) return RedirectToAction(nameof(Index));

        if (ModelState.IsValid)
        {
            // Nếu khách đã đăng nhập, gắn tài khoản của họ vào đơn này
            if (User.Identity?.IsAuthenticated == true)
            {
                order.AppUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            order.OrderDate = DateTime.Now;
            order.Status = "Chờ xác nhận"; // Mặc định trạng thái đơn

            // A. Lưu cái "Vỏ đơn" (Order) vào Database trước để lấy được cái Mã Đơn (Id)
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // B. Mở giỏ hàng ra, lấy từng căn nhà nhét vào bảng Chi Tiết (OrderDetails)
            foreach (var item in cart)
            {
                var detail = new OrderDetail
                {
                    OrderId = order.Id,         // Lấy mã đơn vừa tạo ở trên
                    PropertyId = item.PropertyId,
                    Price = item.Price
                };
                _context.OrderDetails.Add(detail);
            }
            await _context.SaveChangesAsync(); // Lưu toàn bộ chi tiết

            // C. Đặt lịch xong rồi thì phải vứt cái giỏ hàng cũ đi
            HttpContext.Session.Remove(CART_KEY);

            // D. Chuyển hướng sang trang báo Thành công
            return RedirectToAction(nameof(Success));
        }
        
        // Nếu form điền sai (ví dụ sđt không hợp lệ), hiển thị lại form để điền lại
        return View(order);
    }

    // 3. Trang hiển thị lời cảm ơn
    public IActionResult Success()
    {
        return View();
    }
}