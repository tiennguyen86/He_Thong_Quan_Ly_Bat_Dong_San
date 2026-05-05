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
    /// <summary>
    /// Controller quản lý tài khoản người dùng (User Management)
    /// Cho phép Admin: Xem, khóa/mở khóa tài khoản, reset mật khẩu
    /// Hỗ trợ tìm kiếm, lọc theo trạng thái, và phân trang
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Admin")]     // Chỉ Admin được quản lý user
    public class UsersController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public UsersController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// GET: Admin/Users/Index
        /// Danh sách người dùng với các tính năng: tìm kiếm, lọc trạng thái, phân trang
        /// </summary>
        /// <param name="searchString">Từ khóa tìm kiếm theo Username hoặc Email (tùy chọn)</param>
        /// <param name="statusFilter">Lọc theo trạng thái: all, active, locked (tùy chọn)</param>
        /// <param name="page">Số trang hiện tại (mặc định: 1)</param>
        /// <returns>View danh sách user với thông tin phân trang</returns>
        public async Task<IActionResult> Index(string searchString, string statusFilter, int page = 1)
        {
            int pageSize = 5;                                      // 5 user trên 1 trang

            // BƯỚC 1: Khởi tạo query gốc - Lấy tất cả user
            var query = _userManager.Users.AsNoTracking();

            // BƯỚC 2: TÌM KIẾM theo Username hoặc Email
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(u => u.UserName.Contains(searchString) || u.Email.Contains(searchString));
            }

            // BƯỚC 3: LỌC theo trạng thái
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
            {
                if (statusFilter == "active")
                    query = query.Where(u => u.IsActive == true);
                else if (statusFilter == "locked")
                    query = query.Where(u => u.IsActive == false);
            }

            // BƯỚC 4: Đếm số lượng user sau khi lọc
            int totalItems = await query.CountAsync();
            int totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 0;

            // Chống lỗi trang trắng
            if (page > totalPages && totalPages > 0) page = totalPages;
            if (page <= 0) page = 1;

            // BƯỚC 5: Lấy dữ liệu của trang hiện tại
            var users = await query
                .OrderByDescending(u => u.Id)                      // Sort theo ID (tức là user mới nhất)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // BƯỚC 6: Truyền dữ liệu phân trang và filter sang View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalUsers = totalItems;
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentFilter = statusFilter;

            return View(users);
        }

        /// <summary>
        /// POST: Admin/Users/ToggleStatus
        /// Đảo trạng thái của user: Hoạt động ↔ Bị khóa
        /// Admin không thể khóa chính mình
        /// </summary>
        /// <param name="id">ID của user cần đảo trạng thái</param>
        /// <returns>Redirect về Index</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Chống bỏ lỡ: Admin không cho khóa chính mình
            if (user.UserName == User.Identity.Name)
                return RedirectToAction(nameof(Index));

            // Đảo trạng thái Active (true ↔ false)
            user.IsActive = !user.IsActive;

            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }
        
        /// <summary>
        /// POST: Admin/Users/ResetUserPassword
        /// Reset mật khẩu cho user về mật khẩu mới mà Admin nhập
        /// Dùng phương thức ResetPasswordAsync của Identity Framework
        /// </summary>
        /// <param name="userId">ID của user cần reset mật khẩu</param>
        /// <param name="newPassword">Mật khẩu mới mà Admin nhập</param>
        /// <returns>Redirect về Index với thông báo thành công/lỗi</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetUserPassword(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // BƯỚC 1: Tạo 1 cái token (luật chứng thực) cho phép ép đổi mật khẩu
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // BƯỚC 2: Thực hiện reset mật khẩu sang mật khẩu mới
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            // BƯỚC 3: Kiểm tra kết quả
            if (result.Succeeded)
            {
                // Thông báo thành công
                TempData["SuccessMessage"] = $"Đã cấp lại mật khẩu cho tài khoản {user.UserName} thành công!";
            }
            else
            {
                // Thông báo lỗi (nếu mật khẩu không đáp ứng yêu cầu)
                TempData["ErrorMessage"] = "Lỗi! Mật khẩu mới phải có chữ hoa, thường, số và ký tự đặc biệt.";
            }

            return RedirectToAction(nameof(Index));
        }
        
        
    }
}