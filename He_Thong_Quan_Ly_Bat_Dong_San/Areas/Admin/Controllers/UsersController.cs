using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System; 
using He_Thong_Quan_Ly_Bat_Dong_San.Models;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] 
    public class UsersController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public UsersController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        // Danh sách user có PHÂN TRANG + TÌM KIẾM + LỌC TRẠNG THÁI
        public async Task<IActionResult> Index(string searchString, string statusFilter, int page = 1)
        {
            int pageSize = 5; // Giữ nguyên số lượng user mỗi trang

            // 1. Query gốc
            var query = _userManager.Users.AsNoTracking();

            // 2. TÌM KIẾM theo Username hoặc Email
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(u => u.UserName.Contains(searchString) || u.Email.Contains(searchString));
            }

            // 3. LỌC trạng thái
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
            {
                if (statusFilter == "active")
                    query = query.Where(u => u.IsActive == true);
                else if (statusFilter == "locked")
                    query = query.Where(u => u.IsActive == false);
            }

            // 4. Đếm số lượng sau khi lọc
            int totalItems = await query.CountAsync();
            int totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 0;

            // Chống lỗi trang trắng
            if (page > totalPages && totalPages > 0) page = totalPages;
            if (page <= 0) page = 1;

            // 5. Lấy dữ liệu theo trang
            var users = await query
                .OrderByDescending(u => u.Id) // Giữ nguyên sắp xếp của bạn
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 6. Truyền dữ liệu ra View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalUsers = totalItems;
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentFilter = statusFilter;

            return View(users);
        }

        // POST: Admin/Users/ToggleStatus (GIỮ NGUYÊN 100%)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Không cho admin tự khóa chính mình
            if (user.UserName == User.Identity.Name)
                return RedirectToAction(nameof(Index));

            // Đảo trạng thái Active
            user.IsActive = !user.IsActive;

            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }
        
        // POST: Admin/Users/ResetUserPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetUserPassword(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // 1. Tạo 1 cái thẻ bài (Token) cho phép ép đổi mật khẩu
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // 2. Thực hiện đổi sang mật khẩu mới Admin vừa nhập
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Đã cấp lại mật khẩu cho tài khoản {user.UserName} thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Lỗi! Mật khẩu mới phải có chữ hoa, thường, số và ký tự đặc biệt.";
            }

            return RedirectToAction(nameof(Index));
        }
        
        
    }
}