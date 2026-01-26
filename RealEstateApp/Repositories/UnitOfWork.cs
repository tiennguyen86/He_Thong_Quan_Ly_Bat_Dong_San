using RealEstateApp.Data;
using RealEstateApp.Models;

namespace RealEstateApp.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _db;
    public IRepository<BatDongSan> BatDongSan { get; private set; }
    public IRepository<LoaiBatDongSan> LoaiBatDongSan { get; private set; }

    public UnitOfWork(ApplicationDbContext db)
    {
        _db = db;
        BatDongSan = new Repository<BatDongSan>(_db);
        LoaiBatDongSan = new Repository<LoaiBatDongSan>(_db);
    }

    public async Task SaveAsync() => await _db.SaveChangesAsync();
}