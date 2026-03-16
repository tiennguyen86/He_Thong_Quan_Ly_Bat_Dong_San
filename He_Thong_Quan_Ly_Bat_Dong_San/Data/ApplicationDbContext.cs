using He_Thong_Quan_Ly_Bat_Dong_San.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Data;

// Đổi DbContext thành IdentityDbContext<AppUser>
public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Property> Properties { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<PropertyImage> PropertyImages { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    
    
}