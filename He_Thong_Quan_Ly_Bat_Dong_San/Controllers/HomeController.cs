using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Thêm dòng này
using He_Thong_Quan_Ly_Bat_Dong_San.Models;
using He_Thong_Quan_Ly_Bat_Dong_San.Data; // Thêm dòng này

namespace He_Thong_Quan_Ly_Bat_Dong_San.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    // Nhúng ApplicationDbContext vào
    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Thêm tham số categoryId vào hàm Index
    // Thêm tham số searchString vào hàm
    public async Task<IActionResult> Index(int? categoryId, string? searchString)
    {
        var propertiesQuery = _context.Properties.Include(p => p.Category).AsQueryable();

        // XỬ LÝ TÌM KIẾM
        if (!string.IsNullOrEmpty(searchString))
        {
            // Tìm trong Tiêu đề hoặc Địa chỉ
            propertiesQuery = propertiesQuery.Where(p => p.Title.Contains(searchString) || p.Address.Contains(searchString));
            ViewBag.CurrentSearch = searchString; // Giữ lại từ khóa để hiện lên ô input
        }

        if (categoryId.HasValue)
        {
            propertiesQuery = propertiesQuery.Where(p => p.CategoryId == categoryId);
            var category = await _context.Categories.FindAsync(categoryId);
            ViewBag.CurrentCategory = category?.Name;
        }

        var result = await propertiesQuery.ToListAsync();
        return View(result);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
    // Mở trang Chi tiết Bất động sản
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        // Tìm căn nhà có ID tương ứng, nhớ Include cả Category để lấy tên Loại BĐS
        var property = await _context.Properties
            .Include(p => p.Category)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (property == null) return NotFound();

        return View(property);
    }
}