using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using He_Thong_Quan_Ly_Bat_Dong_San.Models;
using He_Thong_Quan_Ly_Bat_Dong_San.Data;
using Microsoft.AspNetCore.Identity;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Controllers;

/// <summary>
/// Controller trang chủ ứng dụng
/// Xử lý hiển thị danh sách BĐS, tìm kiếm, lọc theo danh mục, sắp xếp giá, phân trang
/// </summary>
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// GET: Home/Index
    /// Hiển thị danh sách bất động sản với các tính năng: tìm kiếm, lọc, sắp xếp, phân trang
    /// </summary>
    /// <param name="categoryId">Mã danh mục để lọc (tùy chọn)</param>
    /// <param name="searchString">Từ khóa tìm kiếm trong tiêu đề hoặc địa chỉ (tùy chọn)</param>
    /// <param name="sortOrder">Thứ tự sắp xếp: price_asc, price_desc (tùy chọn)</param>
    /// <param name="page">Số trang hiện tại (mặc định: 1)</param>
    /// <returns>View danh sách BĐS đã lọc, sắp xếp và phân trang</returns>
    public async Task<IActionResult> Index(int? categoryId, string? searchString, string? sortOrder, int page = 1)
    {
        int pageSize = 6; // Hiển thị 6 BĐS trên 1 trang

        // Khởi tạo query cơ bản: Lấy tất cả BĐS, include danh mục, không track để tối ưu performance
        var propertiesQuery = _context.Properties
            .Include(p => p.Category)
            .AsNoTracking()                       // ⚡ Tối ưu hóa: Chỉ đọc, không update
            .AsQueryable();

        // ====== BƯỚC 1: TÌM KIẾM ======
        if (!string.IsNullOrEmpty(searchString))
        {
            // Tìm kiếm trong tiêu đề hoặc địa chỉ (không phân biệt hoa/thường)
            propertiesQuery = propertiesQuery.Where(p =>
                p.Title.Contains(searchString) ||
                p.Address.Contains(searchString));

            // Lưu từ khóa tìm kiếm để hiển thị lại trong view
            ViewBag.CurrentSearch = searchString;
        }

        // ====== BƯỚC 2: LỌC THEO DANH MỤC ======
        if (categoryId.HasValue)
        {
            // Lọc BĐS theo danh mục
            propertiesQuery = propertiesQuery.Where(p => p.CategoryId == categoryId);

            // Lấy tên danh mục để hiển thị
            var category = await _context.Categories.FindAsync(categoryId);

            ViewBag.CurrentCategory = category?.Name;
            ViewBag.CurrentCategoryId = categoryId;
        }

        // ====== BƯỚC 3: SẮP XẾP ======
        switch (sortOrder)
        {
            case "price_asc":           // Giá thấp → cao
                propertiesQuery = propertiesQuery.OrderBy(p => p.Price);
                break;

            case "price_desc":          // Giá cao → thấp
                propertiesQuery = propertiesQuery.OrderByDescending(p => p.Price);
                break;

            default:                    // Mặc định: Mới nhất (ID cao nhất)
                propertiesQuery = propertiesQuery.OrderByDescending(p => p.Id);
                break;
        }

        // Lưu thứ tự sắp xếp hiện tại để form giữ nguyên
        ViewBag.CurrentSort = sortOrder;

        // ====== BƯỚC 4: PHÂN TRANG ======
        // Đếm tổng số BĐS sau khi lọc
        int totalItems = await propertiesQuery.CountAsync();
        
        // Tính tổng số trang (làm tròn lên)
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        // Lấy dữ liệu của trang hiện tại
        var result = await propertiesQuery
            .Skip((page - 1) * pageSize)  // Bỏ qua các BĐS của những trang trước
            .Take(pageSize)               // Lấy đúng số lượng BĐS cần thiết
            .ToListAsync();

        // Lưu thông tin phân trang sang view
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        return View(result);
    }

    /// <summary>
    /// GET: Home/Details/{id}
    /// Hiển thị chi tiết một bất động sản (tiêu đề, giá, mô tả, hình ảnh gallery)
    /// </summary>
    /// <param name="id">Mã bất động sản cần xem</param>
    /// <returns>
    /// - Nếu tìm thấy: View chi tiết BĐS
    /// - Nếu không tìm thấy: 404 Not Found
    /// </returns>
    public async Task<IActionResult> Details(int? id)
    {
        // Kiểm tra ID hợp lệ
        if (id == null) return NotFound();

        // Lấy BĐS từ DB, kéo theo danh mục và danh sách ảnh gallery
        var property = await _context.Properties
            .Include(p => p.Category)
            .Include(p => p.PropertyImages)
            .FirstOrDefaultAsync(m => m.Id == id);

        // Kiểm tra BĐS có tồn tại không
        if (property == null) return NotFound();

        return View(property);
    }

    /// <summary>
    /// [CHỨC NĂNG HỖ TRỢ] GET: Home/CreateSaleAccount
    /// Tạo một role "Sale" và tài khoản nhân viên bán hàng mặc định
    /// CẢNH BÁO: Chỉ dùng để test/phát triển, nên xóa trước khi deploy production
    /// </summary>
    public async Task<IActionResult> CreateSaleAccount(
        [FromServices] UserManager<AppUser> userManager,
        [FromServices] RoleManager<IdentityRole> roleManager)
    {
        // Bước 1: Tạo role "Sale" nếu chưa tồn tại
        if (!await roleManager.RoleExistsAsync("Sale"))
        {
            await roleManager.CreateAsync(new IdentityRole("Sale"));
        }

        // Bước 2: Tạo tài khoản nhân viên
        var saleUser = new AppUser
        {
            UserName = "nhanvien1@gmail.com",
            Email = "nhanvien1@gmail.com",
            FullName = "Nhân viên Chốt Đơn"
        };

        var result = await userManager.CreateAsync(saleUser, "Sale@12345");

        if (result.Succeeded)
        {
            // Bước 3: Gán role "Sale" cho tài khoản này
            await userManager.AddToRoleAsync(saleUser, "Sale");

            return Content(
                "THÀNH CÔNG! Đã tạo tài khoản Sale.\n" +
                "Email: nhanvien1@gmail.com\n" +
                "Mật khẩu: Sale@12345");
        }

        return Content("Có lỗi xảy ra hoặc tài khoản đã tồn tại!");
    }

    /// <summary>
    /// GET: Home/Privacy
    /// Hiển thị trang Chính sách bảo mật
    /// </summary>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// GET: Home/About
    /// Hiển thị trang Về chúng tôi
    /// </summary>
    public IActionResult About()
    {
        return View();
    }

    /// <summary>
    /// GET: Home/Terms
    /// Hiển thị trang Điều khoản dịch vụ
    /// </summary>
    public IActionResult Terms()
    {
        return View();
    }

    /// <summary>
    /// GET: Home/Error
    /// Hiển thị trang lỗi chung
    /// Cache response để tối ưu performance
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }

    /// <summary>
    /// [CHỨC NĂNG HỖ TRỢ] GET: Home/AutoGenerateData
    /// Tự động sinh ra 10 bất động sản mẫu để test phân trang
    /// CẢNH BÁO: Chỉ dùng để test/phát triển, nên xóa trước khi deploy production
    /// </summary>
    public async Task<IActionResult> AutoGenerateData()
    {
        // Kiểm tra có danh mục nào trong DB không
        var category = await _context.Categories.FirstOrDefaultAsync();

        if (category == null)
            return Content("Lỗi: Bạn chưa có Danh mục nào trong Database!");

        // Sinh ra 10 BĐS mẫu
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