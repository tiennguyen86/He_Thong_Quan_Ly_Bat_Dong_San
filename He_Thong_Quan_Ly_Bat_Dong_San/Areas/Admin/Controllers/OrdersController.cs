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
    [Area("Admin")]
    [Authorize(Roles = "Admin, Sale")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Phân trang
        // GET: Admin/Orders
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 6; // Số lượng đơn hàng hiển thị trên 1 trang

            // Đếm tổng số đơn hàng có trong DB
            int totalItems = await _context.Orders.CountAsync();
            
            // Tính tổng số trang (làm tròn lên)
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Chặn lỗi: Nếu nhập trang < 1 thì đưa về trang 1, nếu lớn hơn tổng số trang thì đưa về trang cuối
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // Truy vấn lấy dữ liệu có Skip và Take
            var orders = await _context.Orders
                .Include(o => o.AppUser)
                .OrderByDescending(o => o.OrderDate) // Sắp xếp đơn mới nhất lên đầu
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Ném dữ liệu phân trang sang View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(orders);
        }

        // GET: Admin/Orders/Details/5
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

        // GET: Admin/Orders/Create
        public IActionResult Create()
        {
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Admin/Orders/Create
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

        // GET: Admin/Orders/Edit/5
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

        // POST: Admin/Orders/Edit/5
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
        
        [Authorize(Roles = "Admin")] // THÊM DÒNG NÀY ĐỂ CHỈ SẾP MỚI ĐƯỢC VÀO
        // GET: Admin/Orders/Delete/5
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

        // POST: Admin/Orders/Delete/5
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

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}