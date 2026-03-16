using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using He_Thong_Quan_Ly_Bat_Dong_San.Data;
using System.Text.Json; // thêm để serialize JSON

namespace He_Thong_Quan_Ly_Bat_Dong_San.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Sale")] // Cả sếp và nhân viên đều được xem thống kê
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // ==============================
            // 1. Đếm tổng số liệu
            // ==============================

            ViewBag.TotalProperties = await _context.Properties.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();

            // Đếm riêng đơn đang "Chờ xác nhận"
            ViewBag.PendingOrders = await _context.Orders
                .Where(o => o.Status == "Chờ xác nhận")
                .CountAsync();


            // ==============================
            // 2. Lấy 5 đơn hàng mới nhất
            // ==============================

            var recentOrders = await _context.Orders
                .OrderByDescending(o => o.Id)
                .Take(5)
                .ToListAsync();


            // ==============================
            // 3. THỐNG KÊ BĐS THEO DANH MỤC
            // ==============================

            var categoryStats = await _context.Properties
                .Include(p => p.Category)
                .GroupBy(p => p.Category.Name)
                .Select(g => new
                {
                    CategoryName = g.Key ?? "Chưa phân loại",
                    Count = g.Count(),
                    AvgPrice = g.Average(p => (double)p.Price)
                })
                .ToListAsync();


            // Convert sang JSON để Javascript đọc
            ViewBag.Labels = JsonSerializer.Serialize(
                categoryStats.Select(c => c.CategoryName));

            ViewBag.Counts = JsonSerializer.Serialize(
                categoryStats.Select(c => c.Count));

            ViewBag.AvgPrices = JsonSerializer.Serialize(
                categoryStats.Select(c => c.AvgPrice));


            // ==============================
            // 4. Trả dữ liệu cho View
            // ==============================

            return View(recentOrders);
        }
    }
}