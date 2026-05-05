// =============================================================
// File: Program.cs
// Mục đích: Cấu hình và khởi động ứng dụng ASP.NET Core MVC
// Dự án: Hệ Thống Quản Lý Bất Động Sản
// Tác giả: Tiến - Hào - Hiếu
// Ngày tạo: 05/05/2025
// =============================================================

using He_Thong_Quan_Ly_Bat_Dong_San.Data;
using Microsoft.EntityFrameworkCore;
using He_Thong_Quan_Ly_Bat_Dong_San.Models;  // AppUser - model người dùng tùy chỉnh
using Microsoft.AspNetCore.Identity;          // IdentityRole - hệ thống phân quyền

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------
// 1. CẤU HÌNH DATABASE
// Kết nối SQL Server thông qua chuỗi kết nối trong appsettings.json
// ---------------------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------------------------------------------------------------
// 2. CẤU HÌNH IDENTITY (Xác thực & Phân quyền)
// Sử dụng AppUser thay cho IdentityUser mặc định để mở rộng thêm thuộc tính (VD: FullName)
// ---------------------------------------------------------------
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
    {
        // Nới lỏng yêu cầu mật khẩu để tiện test trong quá trình phát triển
        // LƯU Ý: Nên bật lại các ràng buộc này khi deploy production
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>() // Lưu dữ liệu Identity vào DB
    .AddDefaultTokenProviders();                      // Hỗ trợ token reset mật khẩu, xác nhận email

// ---------------------------------------------------------------
// 3. CẤU HÌNH COOKIE XÁC THỰC
// Điều hướng người dùng khi chưa đăng nhập hoặc không đủ quyền
// ---------------------------------------------------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";           // Chuyển hướng khi chưa đăng nhập
    options.AccessDeniedPath = "/Account/AccessDenied"; // Chuyển hướng khi không đủ quyền truy cập
});

// ---------------------------------------------------------------
// 4. ĐĂNG KÝ CÁC DỊCH VỤ BỔ SUNG
// ---------------------------------------------------------------
builder.Services.AddControllersWithViews(); // Hỗ trợ mô hình MVC
builder.Services.AddHttpContextAccessor();  // Truy cập HttpContext trong các service

// Cấu hình Session (dùng cho giỏ hàng / trạng thái tạm thời)
builder.Services.AddDistributedMemoryCache(); // Bộ nhớ cache cho session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Phiên làm việc hết hạn sau 30 phút không hoạt động
    options.Cookie.HttpOnly = true;    // Ngăn JavaScript truy cập cookie (bảo mật XSS)
    options.Cookie.IsEssential = true; // Cookie bắt buộc, không cần xin phép GDPR
});

var app = builder.Build();

// ===============================================================
// SEEDING DATA — TẠO DỮ LIỆU MẶC ĐỊNH KHI KHỞI ĐỘNG LẦN ĐẦU
// Đảm bảo hệ thống luôn có tài khoản Admin để quản trị
// ===============================================================
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    // Bước 1: Tạo Role "Admin" nếu chưa tồn tại trong CSDL
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // Bước 2: Tạo tài khoản Admin mặc định nếu chưa tồn tại
    var adminEmail = "admin@realestate.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var newAdmin = new AppUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Quản Trị Viên Tối Cao",
            EmailConfirmed = true // Bỏ qua bước xác nhận email
        };

        // Tạo tài khoản với mật khẩu mặc định
        // MẬT KHẨU: Admin@123 — đổi lại trước khi deploy!
        var result = await userManager.CreateAsync(newAdmin, "Admin@123");

        if (result.Succeeded)
        {
            // Gán quyền Admin cho tài khoản vừa tạo
            await userManager.AddToRoleAsync(newAdmin, "Admin");
        }
    }
}
// ===============================================================
// KẾT THÚC SEEDING DATA
// ===============================================================

// ---------------------------------------------------------------
// 5. CẤU HÌNH MIDDLEWARE PIPELINE
// Thứ tự middleware RẤT QUAN TRỌNG, không được đổi chỗ tùy tiện
// ---------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    // Chỉ bật trang lỗi thân thiện khi chạy production
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();    // Phục vụ file tĩnh: CSS, JS, hình ảnh (wwwroot/)
app.UseRouting();        // Xác định route phù hợp cho request
app.UseSession();        // Kích hoạt Session — phải đặt TRƯỚC Authentication
app.UseAuthentication(); // Xác thực: "Bạn là ai?" (đọc cookie, giải mã token)
app.UseAuthorization();  // Phân quyền: "Bạn được làm gì?" (kiểm tra role, policy)

// ---------------------------------------------------------------
// 6. CẤU HÌNH ROUTING
// ---------------------------------------------------------------

// Route cho Area (Admin khu vực riêng biệt)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Route mặc định cho toàn bộ ứng dụng
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();