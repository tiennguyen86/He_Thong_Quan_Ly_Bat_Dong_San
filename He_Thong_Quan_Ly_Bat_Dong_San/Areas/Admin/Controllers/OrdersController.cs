using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using He_Thong_Quan_Ly_Bat_Dong_San.Data;
using He_Thong_Quan_Ly_Bat_Dong_San.Models;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller CRUD quản lý đơn lịch hẹn (Order)
    /// Cho phép Admin, Sale xem/sửa/xóa đơn hàng
    /// Admin có thêm quyền xóa đơn hàng
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Admin, Sale")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: Admin/Orders/Index
        /// Hiển thị danh sách đơn hàng với phân trang
        /// Sắp xếp theo ngày đặt mới nhất trước
        /// </summary>
        /// <param name="page">Số trang hiện tại (mặc định: 1)</param>
        /// <returns>View danh sách đơn hàng với thông tin phân trang</returns>
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 6;                                          // 6 đơn hàng trên 1 trang

            // Đếm tổng số đơn hàng
            int totalItems = await _context.Orders.CountAsync();
            
            // Tính tổng số trang (làm tròn lên)
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Chặn lỗi trang vô lý
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // Truy vấn lấy dữ liệu có phân trang
            var orders = await _context.Orders
                .Include(o => o.AppUser)                          // Kéo theo thông tin người dùng
                .OrderByDescending(o => o.OrderDate)              // Sắp xếp từ mới nhất
                .Skip((page - 1) * pageSize)                      // Bỏ qua các đơn của trang trước
                .Take(pageSize)                                   // Lấy đúng số lượng cần
                .ToListAsync();

            // Truyền thông tin phân trang sang View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(orders);
        }

        /// <summary>
        /// GET: Admin/Orders/Details/{id}
        /// Hiển thị chi tiết một đơn hàng (bao gồm thông tin khách hàng)
        /// </summary>
        /// <param name="id">Mã đơn hàng cần xem</param>
        /// <returns>View chi tiết đơn hàng, hoặc 404 nếu không tìm thấy</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        /// <summary>
        /// GET: Admin/Orders/Create
        /// Hiển thị form tạo đơn hàng mới (Admin tạo hộ khách)
        /// </summary>
        /// <returns>View form tạo đơn hàng</returns>
        public IActionResult Create()
        {
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        /// <summary>
        /// POST: Admin/Orders/Create
        /// Xử lý form tạo đơn hàng mới
        /// </summary>
        /// <param name="order">Dữ liệu đơn hàng từ form</param>
        /// <returns>
        /// - Nếu thành công: Redirect về Index
        /// - Nếu lỗi validation: Hiển thị lại form
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AppUserId,CustomerName,PhoneNumber,OrderDate,Notes,Status")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", order.AppUserId);
            return View(order);
        }

        /// <summary>
        /// GET: Admin/Orders/Edit/{id}
        /// Hiển thị form sửa thông tin đơn hàng (VD: thay đổi trạng thái)
        /// </summary>
        /// <param name="id">Mã đơn hàng cần sửa</param>
        /// <returns>View form sửa đơn hàng, hoặc 404 nếu không tìm thấy</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", order.AppUserId);
            return View(order);
        }

        /// <summary>
        /// POST: Admin/Orders/Edit/{id}
        /// Xử lý form sửa thông tin đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng cần sửa</param>
        /// <param name="order">Dữ liệu đơn hàng đã sửa từ form</param>
        /// <returns>
        /// - Nếu thành công: Redirect về Index
        /// - Nếu lỗi concurrency: Thông báo lỗi
        /// - Nếu lỗi validation: Hiển thị lại form
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUserId,CustomerName,PhoneNumber,OrderDate,Notes,Status")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", order.AppUserId);
            return View(order);
        }
        
        /// <summary>
        /// GET: Admin/Orders/Delete/{id}
        /// Hiển thị trang xác nhận xóa đơn hàng
        /// CẢNH BÁO: Chỉ Admin mới được xóa đơn, Sale không được
        /// </summary>
        /// <param name="id">Mã đơn hàng cần xóa</param>
        /// <returns>View xác nhận xóa, hoặc 404 nếu không tìm thấy</returns>
        [Authorize(Roles = "Admin")]     // Chỉ Admin được xóa đơn
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        /// <summary>
        /// POST: Admin/Orders/Delete/{id}
        /// Xử lý xóa đơn hàng khỏi DB
        /// Chỉ Admin được thực hiện
        /// </summary>
        /// <param name="id">Mã đơn hàng cần xóa</param>
        /// <returns>Redirect về Index</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Phương thức hỗ trợ kiểm tra đơn hàng có tồn tại không
        /// </summary>
        /// <param name="id">Mã đơn hàng cần kiểm tra</param>
        /// <returns>True nếu tồn tại, False nếu không</returns>
        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}