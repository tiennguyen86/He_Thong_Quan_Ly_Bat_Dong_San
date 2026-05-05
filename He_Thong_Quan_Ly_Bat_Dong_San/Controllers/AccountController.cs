using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using He_Thong_Quan_Ly_Bat_Dong_San.Models;
using He_Thong_Quan_Ly_Bat_Dong_San.ViewModels;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Controllers
{
    /// <summary>
    /// Controller quản lý xác thực người dùng (Authentication)
    /// Xử lý các chức năng: Đăng ký, Đăng nhập, Đăng xuất
    /// </summary>
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        /// <summary>
        /// Constructor: Nhận UserManager và SignInManager từ Dependency Injection
        /// </summary>
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// GET: Account/Register
        /// Hiển thị form đăng ký tài khoản mới
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// POST: Account/Register
        /// Xử lý form đăng ký: Tạo tài khoản mới và đăng nhập tự động
        /// </summary>
        /// <param name="model">Dữ liệu từ form (FullName, Email, Password)</param>
        /// <returns>
        /// - Nếu thành công: Redirect về trang chủ sau khi đăng nhập
        /// - Nếu lỗi: Hiển thị lại form với thông báo lỗi
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                // Tạo object AppUser mới từ dữ liệu form
                var user = new AppUser
                {
                    UserName = model.Email,      // Dùng email làm username
                    Email = model.Email,
                    FullName = model.FullName,
                    IsActive = true               // Kích hoạt tài khoản ngay lập tức
                };

                // Gọi Identity Framework để tạo tài khoản
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Đăng nhập tự động sau khi tạo tài khoản thành công
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                // Nếu tạo tài khoản thất bại, thêm lỗi vào ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            // Trả về form nếu dữ liệu không hợp lệ
            return View(model);
        }

        /// <summary>
        /// GET: Account/Login
        /// Hiển thị form đăng nhập
        /// </summary>
        /// <param name="returnUrl">URL để quay lại sau khi đăng nhập thành công (nếu có)</param>
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// POST: Account/Login
        /// Xử lý form đăng nhập: Kiểm tra email/password và tạo authenticated session
        /// </summary>
        /// <param name="model">Dữ liệu từ form (Email, Password, RememberMe)</param>
        /// <param name="returnUrl">URL để quay lại sau khi đăng nhập thành công</param>
        /// <returns>
        /// - Nếu thành công: Redirect đến returnUrl hoặc trang chủ
        /// - Nếu lỗi: Hiển thị lại form với thông báo lỗi
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Tìm user theo email
                var user = await _userManager.FindByEmailAsync(model.Email);

                // Kiểm tra xem user tồn tại và không bị khóa
                if (user == null || !user.IsActive)
                {
                    ModelState.AddModelError("", "Tài khoản không tồn tại hoặc đã bị khóa.");
                    return View(model);
                }

                // Kiểm tra mật khẩu
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false
                );

                if (result.Succeeded)
                {
                    // Nếu có returnUrl hợp lệ, quay lại URL đó
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }

                    // Ngược lại quay về trang chủ
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác.");
            }

            return View(model);
        }

        /// <summary>
        /// POST: Account/Logout
        /// Xử lý đăng xuất: Xóa authenticated cookie
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// GET: Account/AccessDenied
        /// Hiển thị trang lỗi khi người dùng không đủ quyền truy cập
        /// </summary>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}