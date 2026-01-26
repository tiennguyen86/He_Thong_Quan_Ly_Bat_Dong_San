using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Models;

namespace RealEstateApp.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // 2 dòng này để khai báo bảng mới 
    public DbSet<LoaiBatDongSan> LoaiBatDongSans { get; set; }
    public DbSet<BatDongSan> BatDongSans { get; set; }
}