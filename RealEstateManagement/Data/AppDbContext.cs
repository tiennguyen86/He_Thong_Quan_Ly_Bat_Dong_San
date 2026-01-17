using Microsoft.EntityFrameworkCore;
using RealEstateManagement.Models;

namespace RealEstateManagement.Data;
// Cầu nối cơ sở dữ liệu 

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Khai báo bảng Properties
    public DbSet<Property> Properties { get; set; }
}