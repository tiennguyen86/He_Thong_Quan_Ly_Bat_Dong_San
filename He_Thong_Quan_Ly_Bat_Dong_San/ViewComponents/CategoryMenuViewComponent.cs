using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using He_Thong_Quan_Ly_Bat_Dong_San.Data;

namespace He_Thong_Quan_Ly_Bat_Dong_San.ViewComponents;

public class CategoryMenuViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public CategoryMenuViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    // Hàm này sẽ tự động chạy khi ViewComponent được gọi
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var categories = await _context.Categories.ToListAsync();
        return View(categories); // Ném dữ liệu sang View
    }
}