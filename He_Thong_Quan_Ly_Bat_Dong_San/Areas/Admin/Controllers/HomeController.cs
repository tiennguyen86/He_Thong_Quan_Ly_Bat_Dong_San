using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using He_Thong_Quan_Ly_Bat_Dong_San.Data;
using System.Text.Json;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller Admin Dashboard
    /// Hiển thị các thống kê tổng hợp: số lượng BĐS, danh mục, đơn hàng, v.v...
    /// Chỉ cho phép Admin hoặc nhân viên Sale (người dùng có role Admin, Sale)
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Admin,Sale")]     // Cả Admin và Sale đều xem được thống kê
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: Admin/Home/Index
        /// Hiển thị dashboard với các thống kê chính và biểu đồ
        /// </summary>
        /// <returns>
        /// Dashboard với các ViewBag chứa:
        /// - TotalProperties, TotalCategories, TotalOrders, PendingOrders
        /// - Danh sách 5 đơn hàng mới nhất
        /// - JSON data cho biểu đồ thống kê
        /// </returns>
        public async Task<IActionResult> Index()
        {
            // ===============================================
            // 1. THỐNG KÊ CƠ BẢN - ĐẾM TỔNG SỐ
            // ===============================================

            ViewBag.TotalProperties = await _context.Properties.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();

            // Đếm riêng số đơn đang chờ xác nhận (chưa liên hệ)
            ViewBag.PendingOrders = await _context.Orders
                .Where(o => o.Status == "Chờ xác nhận")
                .CountAsync();


            // ===============================================
            // 2. DANH SÁCH 5 ĐƠN HÀ NG MỚI NHẤT
            // ===============================================

            var recentOrders = await _context.Orders
                .OrderByDescending(o => o.Id)
                .Take(5)
                .ToListAsync();


            // ===============================================
            // 3. THỐNG KÊ NÂNG CAO - BĐS THEO DANH MỤC
            // Group by danh mục để tính số lượng, giá trung bình
            // ===============================================

            var categoryStats = await _context.Properties
                .Include(p => p.Category)
                .GroupBy(p => p.Category.Name)
                .Select(g => new
                {
                    CategoryName = g.Key ?? "Chưa phân loại",
                    Count = g.Count(),                          // Số BĐS trong danh mục
                    AvgPrice = g.Average(p => (double)p.Price)  // Giá trung bình
                })
                .ToListAsync();


            // ===============================================
            // 4. CHUYỂN DỮ LIỆU SANG JSON ĐỂ JAVASCRIPT ĐỌC
            // Để tạo biểu đồ bằng Chart.js hoặc thư viện khác
            // ===============================================

            // Lấy danh sách tên danh mục
            ViewBag.Labels = JsonSerializer.Serialize(
                categoryStats.Select(c => c.CategoryName));

            // Lấy danh sách số lượng BĐS theo danh mục
            ViewBag.Counts = JsonSerializer.Serialize(
                categoryStats.Select(c => c.Count));

            // Lấy danh sách giá trung bình theo danh mục
            ViewBag.AvgPrices = JsonSerializer.Serialize(
                categoryStats.Select(c => c.AvgPrice));


            // ===============================================
            // 5. TRUYỀN DỮ LIỆU SANG VIEW
            // ===============================================

            return View(recentOrders);
        }
    }
}