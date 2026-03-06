using He_Thong_Quan_Ly_Bat_Dong_San.Data;
using Microsoft.EntityFrameworkCore;
using He_Thong_Quan_Ly_Bat_Dong_San.Models;       // Bổ sung dòng này để nhận diện AppUser
using Microsoft.AspNetCore.Identity;              // Bổ sung dòng này để nhận diện IdentityRole

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options => 
    {
        // Cấu hình password nhẹ nhàng xíu cho dễ test lúc code
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Cấu hình cookie đăng nhập
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login"; // Đường dẫn khi chưa đăng nhập
    options.AccessDeniedPath = "/Account/AccessDenied"; // Đường dẫn khi bị cấm quyền
});

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Giỏ hàng tồn tại 30 phút
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();
// --- ĐOẠN CODE SEEDING DATA (TẠO ADMIN MẶC ĐỊNH) ---
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    // 1. Tạo Role "Admin" nếu chưa có trong CSDL
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // 2. Tạo tài khoản Admin mặc định
    var adminEmail = "admin@realestate.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var newAdmin = new AppUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Quản Trị Viên Tối Cao",
            EmailConfirmed = true
        };
        // Mật khẩu cho tài khoản admin này là: Admin@123
        var result = await userManager.CreateAsync(newAdmin, "Admin@123");
        if (result.Succeeded)
        {
            // Bơm quyền "Admin" cho tài khoản này
            await userManager.AddToRoleAsync(newAdmin, "Admin");
        }
    }
}
// --- KẾT THÚC ĐOẠN SEEDING ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication(); // Xác thực người dùng (Ai đây?)
app.UseAuthorization();  // Cấp quyền (Được làm gì?)


app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();