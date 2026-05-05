using Microsoft.AspNetCore.Mvc;
using He_Thong_Quan_Ly_Bat_Dong_San.Data;
using He_Thong_Quan_Ly_Bat_Dong_San.Models;
using He_Thong_Quan_Ly_Bat_Dong_San.Helpers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Controllers;

/// <summary>
/// Controller quản lý giỏ hàng (Cart) và đơn lịch hẹn
/// Cho phép khách chọn BĐS, tạo đơn lịch hẹn, xem lịch sử đặt lịch
/// </summary>
public class CartController : Controller
{
    private readonly ApplicationDbContext _context;

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Key chứa trong Session để lưu danh sách giỏ hàng
    /// Format: Danh sách CartItem được serialize thành JSON
    /// </summary>
    private const string CART_KEY = "MyCart";

    /// <summary>
    /// GET: Cart/Index
    /// Hiển thị danh sách các bất động sản trong giỏ hàng
    /// </summary>
    /// <returns>View danh sách giỏ hàng (có thể rỗng nếu chưa chọn BĐS nào)</returns>
    public IActionResult Index()
    {
        // Lấy danh sách từ Session, nếu không có thì khởi tạo list rỗng
        var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
        return View(cart);
    }

    /// <summary>
    /// GET: Cart/AddToCart/{id}
    /// Thêm một bất động sản vào giỏ hàng
    /// Không cho phép thêm cùng một BĐS 2 lần
    /// </summary>
    /// <param name="id">Mã bất động sản cần thêm</param>
    /// <returns>Redirect về trang giỏ hàng sau khi thêm</returns>
    public IActionResult AddToCart(int id)
    {
        // Bước 1: Kiểm tra BĐS có tồn tại trong DB
        var property = _context.Properties.Find(id);
        if (property == null) return NotFound();

        // Bước 2: Lấy giỏ hàng hiện tại từ Session
        var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();

        // Bước 3: Kiểm tra xem BĐS đã có trong giỏ chưa (không cho add 1 BĐS 2 lần)
        var item = cart.FirstOrDefault(c => c.PropertyId == id);
        if (item == null)
        {
            // Nếu chưa có thì thêm nó vào giỏ
            cart.Add(new CartItem
            {
                PropertyId = property.Id,
                Title = property.Title,
                Price = property.Price,
                ImageUrl = property.ImageUrl ?? "/images/default.jpg"
            });
        }

        // Bước 4: Lưu giỏ hàng (có thêm BĐS mới) vào Session
        HttpContext.Session.Set(CART_KEY, cart);

        // Bước 5: Redirect về trang giỏ hàng để khách xem
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// GET: Cart/RemoveFromCart/{id}
    /// Xóa một bất động sản khỏi giỏ hàng
    /// </summary>
    /// <param name="id">Mã bất động sản cần xóa</param>
    /// <returns>Redirect về trang giỏ hàng</returns>
    public IActionResult RemoveFromCart(int id)
    {
        // Lấy giỏ hàng hiện tại
        var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
        
        // Tìm BĐS cần xóa
        var itemToRemove = cart.FirstOrDefault(c => c.PropertyId == id);
        if (itemToRemove != null)
        {
            // Xóa khỏi danh sách
            cart.Remove(itemToRemove);
            
            // Lưu lại Session
            HttpContext.Session.Set(CART_KEY, cart);
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// GET: Cart/Checkout
    /// Hiển thị form nhập thông tin khách hàng để xác nhận đặt lịch hẹn
    /// </summary>
    /// <returns>
    /// - Nếu giỏ rỗng: Redirect về trang giỏ hàng
    /// - Nếu giỏ có dữ liệu: Hiển thị form điền thông tin Order
    /// </returns>
    [HttpGet]
    public IActionResult Checkout()
    {
        // Lấy danh sách BĐS từ giỏ
        var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
        
        // Kiểm tra giỏ có trống không
        if (!cart.Any()) 
        {
            return RedirectToAction(nameof(Index));
        }
        
        // Truyền object Order rỗng để form điền dữ liệu
        return View(new Order());
    }

    /// <summary>
    /// POST: Cart/Checkout
    /// Xử lý form checkout: Tạo đơn lịch hẹn và chi tiết đơn hàng
    /// </summary>
    /// <param name="order">Dữ liệu từ form (CustomerName, PhoneNumber, Notes)</param>
    /// <returns>
    /// - Nếu thành công: Redirect sang trang Success
    /// - Nếu lỗi validation: Hiển thị lại form
    /// - Nếu giỏ rỗng: Redirect về giỏ hàng
    /// </returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(Order order)
    {
        // Lấy danh sách BĐS từ giỏ
        var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
        
        // Kiểm tra giỏ còn dữ liệu không
        if (!cart.Any()) return RedirectToAction(nameof(Index));

        if (ModelState.IsValid)
        {
            // Nếu khách đã đăng nhập, gắn thông tin tài khoản vào đơn
            if (User.Identity?.IsAuthenticated == true)
            {
                order.AppUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            // Gán thời gian đặt lịch hiện tại
            order.OrderDate = DateTime.Now;
            
            // Gán trạng thái mặc định
            order.Status = "Chờ xác nhận";

            // Bước A: Lưu "vỏ đơn" (Order) vào Database trước để lấy mã đơn (ID)
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Bước B: Duyệt từng BĐS trong giỏ, tạo OrderDetail cho mỗi BĐS
            foreach (var item in cart)
            {
                var detail = new OrderDetail
                {
                    OrderId = order.Id,              // Lấy mã đơn vừa tạo
                    PropertyId = item.PropertyId,    // BĐS từ giỏ
                    Price = item.Price               // Giá lúc đặt
                };
                _context.OrderDetails.Add(detail);
            }
            // Lưu tất cả chi tiết vào DB
            await _context.SaveChangesAsync();

            // Bước C: Xóa giỏ hàng đã cũ khỏi Session
            HttpContext.Session.Remove(CART_KEY);

            // Bước D: Redirect sang trang cảm ơn
            return RedirectToAction(nameof(Success));
        }
        
        // Nếu form điền sai (VD: sđt không hợp lệ), hiển thị lại form
        return View(order);
    }

    /// <summary>
    /// GET: Cart/Success
    /// Hiển thị trang cảm ơn sau khi đặt lịch hẹn thành công
    /// </summary>
    /// <returns>View trang thông báo thành công</returns>
    public IActionResult Success()
    {
        return View();
    }
    
    /// <summary>
    /// GET: Cart/History
    /// Hiển thị lịch sử tất cả đơn lịch hẹn của người dùng đã đăng nhập
    /// Yêu cầu bắt buộc phải đăng nhập
    /// </summary>
    /// <returns>
    /// - Nếu có đơn: Danh sách đơn hàng của người dùng (sắp xếp mới nhất trước)
    /// - Nếu không có: Danh sách rỗng
    /// </returns>
    [Authorize]
    public async Task<IActionResult> History()
    {
        // Lấy tên/email của người dùng đang đăng nhập
        var currentUser = User.Identity.Name;

        // Lấy danh sách đơn hàng
        // LƯU Ý: Nếu bảng Order không có trường Email/Username, hãy bỏ comment dòng Where để lấy tất cả đơn
        var myOrders = await _context.Orders
            // .Where(o => o.Email == currentUser) // Bỏ comment dòng này nếu DB lưu Email khách khi đặt
            .OrderByDescending(o => o.Id)     // Sắp xếp đơn mới nhất lên đầu
            .ToListAsync();

        return View(myOrders);
    }
}