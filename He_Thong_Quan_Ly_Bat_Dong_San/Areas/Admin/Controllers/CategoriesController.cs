using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using He_Thong_Quan_Ly_Bat_Dong_San.Data;
using He_Thong_Quan_Ly_Bat_Dong_San.Models;
using Microsoft.AspNetCore.Authorization;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller CRUD quản lý danh mục bất động sản (Category)
    /// Cho phép Admin tạo, sửa, xóa các loại danh mục
    /// Chỉ Admin mới được vào (bảo vệ bằng [Authorize])
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Admin")]     // Chỉ Admin mới được phép quản lý danh mục
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: Admin/Categories/Index
        /// Hiển thị danh sách tất cả danh mục
        /// </summary>
        /// <returns>View danh sách danh mục</returns>
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        /// <summary>
        /// GET: Admin/Categories/Details/{id}
        /// Hiển thị chi tiết một danh mục cụ thể
        /// </summary>
        /// <param name="id">Mã danh mục cần xem</param>
        /// <returns>View chi tiết danh mục, hoặc 404 nếu không tìm thấy</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        /// <summary>
        /// GET: Admin/Categories/Create
        /// Hiển thị form tạo danh mục mới
        /// </summary>
        /// <returns>View form tạo danh mục</returns>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// POST: Admin/Categories/Create
        /// Xử lý form tạo danh mục mới
        /// </summary>
        /// <param name="category">Dữ liệu danh mục từ form (Name, Description)</param>
        /// <returns>
        /// - Nếu thành công: Redirect về Index
        /// - Nếu lỗi validation: Hiển thị lại form với thông báo lỗi
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Category category)
        {
            if (ModelState.IsValid)
            {
                // Thêm danh mục mới vào DB
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        /// <summary>
        /// GET: Admin/Categories/Edit/{id}
        /// Hiển thị form sửa thông tin danh mục
        /// </summary>
        /// <param name="id">Mã danh mục cần sửa</param>
        /// <returns>View form sửa danh mục, hoặc 404 nếu không tìm thấy</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        /// <summary>
        /// POST: Admin/Categories/Edit/{id}
        /// Xử lý form sửa thông tin danh mục
        /// </summary>
        /// <param name="id">Mã danh mục cần sửa</param>
        /// <param name="category">Dữ liệu danh mục đã sửa từ form</param>
        /// <returns>
        /// - Nếu thành công: Redirect về Index
        /// - Nếu lỗi concurrency: Thông báo lỗi
        /// - Nếu lỗi validation: Hiển thị lại form
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật danh mục vào DB
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Xử lý lỗi khi hai người edit cùng lúc
                    if (!CategoryExists(category.Id))
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
            return View(category);
        }

        /// <summary>
        /// GET: Admin/Categories/Delete/{id}
        /// Hiển thị trang xác nhận xóa danh mục
        /// </summary>
        /// <param name="id">Mã danh mục cần xóa</param>
        /// <returns>View xác nhận xóa, hoặc 404 nếu không tìm thấy</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        /// <summary>
        /// POST: Admin/Categories/Delete/{id}
        /// Xử lý xóa danh mục khỏi DB
        /// CẢNH BÁO: Xóa danh mục sẽ ảnh hưởng đến các BĐS liên quan
        /// </summary>
        /// <param name="id">Mã danh mục cần xóa</param>
        /// <returns>Redirect về Index</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Phương thức hỗ trợ kiểm tra danh mục có tồn tại không
        /// </summary>
        /// <param name="id">Mã danh mục cần kiểm tra</param>
        /// <returns>True nếu tồn tại, False nếu không</returns>
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
