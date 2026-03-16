using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using He_Thong_Quan_Ly_Bat_Dong_San.Models;
using He_Thong_Quan_Ly_Bat_Dong_San.Data;
using Microsoft.AspNetCore.Identity;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    // Nhúng ApplicationDbContext vào
    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Thêm tham số categoryId
    // Thêm tham số searchString
    // Thêm tham số sortOrder
    // Thêm tham số page
    public async Task<IActionResult> Index(int? categoryId, string? searchString, string? sortOrder, int page = 1)
    {
        int pageSize = 6; // Hiển thị 6 căn nhà trên 1 trang

        var propertiesQuery = _context.Properties
            .Include(p => p.Category)
            .AsNoTracking() // ⚡ Tăng tốc khi chỉ đọc dữ liệu
            .AsQueryable();

        // 1. Lọc theo tìm kiếm
        if (!string.IsNullOrEmpty(searchString))
        {
            propertiesQuery = propertiesQuery.Where(p =>
                p.Title.Contains(searchString) ||
                p.Address.Contains(searchString));

            ViewBag.CurrentSearch = searchString;
        }

        // 2. Lọc theo danh mục
        if (categoryId.HasValue)
        {
            propertiesQuery = propertiesQuery.Where(p => p.CategoryId == categoryId);

            var category = await _context.Categories.FindAsync(categoryId);

            ViewBag.CurrentCategory = category?.Name;
            ViewBag.CurrentCategoryId = categoryId;
        }

        // 3. Sắp xếp dữ liệu
        switch (sortOrder)
        {
            case "price_asc": // Giá thấp -> cao
                propertiesQuery = propertiesQuery.OrderBy(p => p.Price);
                break;

            case "price_desc": // Giá cao -> thấp
                propertiesQuery = propertiesQuery.OrderByDescending(p => p.Price);
                break;

            default: // Mặc định: Mới nhất
                propertiesQuery = propertiesQuery.OrderByDescending(p => p.Id);
                break;
        }

        ViewBag.CurrentSort = sortOrder;

        // 4. Đếm tổng số lượng và tính số trang
        int totalItems = await propertiesQuery.CountAsync();
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        // 5. Cắt lấy dữ liệu của trang hiện tại
        var result = await propertiesQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 6. Truyền thông tin trang sang View
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        return View(result);
    }

    // GET: Home/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        // Lấy thông tin nhà + kéo theo danh mục và album ảnh
        var property = await _context.Properties
            .Include(p => p.Category)
            .Include(p => p.PropertyImages)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (property == null) return NotFound();

        return View(property);
    }

    // TÀ THUẬT: Tự động tạo chức vụ Sale và 1 tài khoản Nhân viên
    public async Task<IActionResult> CreateSaleAccount(
        [FromServices] UserManager<AppUser> userManager,
        [FromServices] RoleManager<IdentityRole> roleManager)
    {
        // 1. Tạo chức vụ "Sale" trong Database nếu chưa có
        if (!await roleManager.RoleExistsAsync("Sale"))
        {
            await roleManager.CreateAsync(new IdentityRole("Sale"));
        }

        // 2. Tạo tài khoản cho nhân viên
        var saleUser = new AppUser
        {
            UserName = "nhanvien1@gmail.com",
            Email = "nhanvien1@gmail.com",
            FullName = "Nhân viên Chốt Đơn"
        };

        var result = await userManager.CreateAsync(saleUser, "Sale@12345");

        if (result.Succeeded)
        {
            // 3. Gắn mác "Sale" cho tài khoản này
            await userManager.AddToRoleAsync(saleUser, "Sale");

            return Content(
                "THÀNH CÔNG! Đã tạo tài khoản Sale.\n" +
                "Email: nhanvien1@gmail.com\n" +
                "Mật khẩu: Sale@12345");
        }

        return Content("Có lỗi xảy ra hoặc tài khoản đã tồn tại!");
    }

    public IActionResult Privacy()
    {
        return View();
    }
    public IActionResult About()
    {
        return View();
    }
    public IActionResult Terms()
    {
        return View();
    }
    
    

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }

    // TÀ THUẬT: Tự động sinh ra 10 căn nhà để test phân trang
    public async Task<IActionResult> AutoGenerateData()
    {
        var category = await _context.Categories.FirstOrDefaultAsync();

        if (category == null)
            return Content("Lỗi: Bạn chưa có Danh mục nào trong Database!");

        for (int i = 1; i <= 10; i++)
        {
            var newProperty = new Property
            {
                Title = $"Siêu phẩm tự động sinh số {i} - View hồ Tây",
                Price = 1500000000m + (i * 150000000m),
                Area = 50 + (i * 5),
                Address = $"Số {i} Đường Tự Động, Quận {i % 5 + 1}, TP.HCM",
                CategoryId = category.Id,
                ImageUrl = ""
            };

            _context.Properties.Add(newProperty);
        }

        await _context.SaveChangesAsync();

        return Content(
            "TÀ THUẬT THÀNH CÔNG! Đã đẻ ra 10 căn nhà.\n" +
            "Hãy xóa /Home/AutoGenerateData trên thanh địa chỉ\n" +
            "và quay lại Trang chủ để xem thành quả nhé!");
    }
}