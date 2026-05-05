using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using He_Thong_Quan_Ly_Bat_Dong_San.Data;

namespace He_Thong_Quan_Ly_Bat_Dong_San.ViewComponents;

/// <summary>
/// ViewComponent dùng để render menu danh mục
/// Thường được gọi từ Layout/_Layout.cshtml để hiển thị menu lọc danh mục
/// </summary>
public class CategoryMenuViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Constructor: Nhận ApplicationDbContext từ Dependency Injection
    /// </summary>
    /// <param name="context">Database context để query danh mục</param>
    public CategoryMenuViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Phương thức chính được gọi tự động khi ViewComponent được render
    /// Lấy danh sách tất cả danh mục từ database
    /// </summary>
    /// <returns>View components result với danh sách danh mục</returns>
    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Lấy toàn bộ danh mục từ database
        var categories = await _context.Categories.ToListAsync();
        
        // Truyền dữ liệu sang View ~/Views/Shared/Components/CategoryMenu/Default.cshtml
        return View(categories);
    }
}