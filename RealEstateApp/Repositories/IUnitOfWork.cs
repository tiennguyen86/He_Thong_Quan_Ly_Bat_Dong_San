using RealEstateApp.Models;

namespace RealEstateApp.Repositories;

public interface IUnitOfWork
{
    IRepository<BatDongSan> BatDongSan { get; }
    IRepository<LoaiBatDongSan> LoaiBatDongSan { get; }
    Task SaveAsync();
}