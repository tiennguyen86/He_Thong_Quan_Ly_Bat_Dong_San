using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RealEstateApp.Models;
using RealEstateApp.Repositories;

namespace RealEstateApp.Areas.Admin.Controllers;

[Area("Admin")]
public class BatDongSanController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public BatDongSanController(IUnitOfWork uow, IWebHostEnvironment webHostEnvironment)
    {
        _uow = uow;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _uow.BatDongSan.GetAllAsync(includeProperties: "LoaiBatDongSan");
        return View(list);
    }

   // GET: Admin/BatDongSan/Create
    public async Task<IActionResult> Create()
    {
        // Thống nhất dùng tên "DsLoai"
        // var danhSachLoai = await _uow.LoaiBatDongSan.GetAllAsync();
        
        // Tạo dữ liệu giả để test View
        var listTam = new List<LoaiBatDongSan> {
            new LoaiBatDongSan { Id = 1, TenLoai = "TEST: Chung cư" },
            new LoaiBatDongSan { Id = 2, TenLoai = "TEST: Nhà đất" }
        };
        ViewBag.DsLoai = new SelectList(listTam, "Id", "TenLoai");
        return View();
    }
    
    // // GET: Admin/BatDongSan/Create
    // public async Task<IActionResult> Create()
    // {
    //     // --- TEST CỨNG (Dữ liệu giả để kiểm tra giao diện) ---
    //     var listTam = new List<LoaiBatDongSan> {
    //         new LoaiBatDongSan { Id = 1, TenLoai = "TEST: Chung cư" },
    //         new LoaiBatDongSan { Id = 2, TenLoai = "TEST: Nhà đất" }
    //     };
    //
    //     // LỖI CỦA BẠN NẰM Ở ĐÂY: Phải dùng "listTam", đừng dùng "danhSachLoai"
    //     ViewBag.DsLoai = new SelectList(listTam, "Id", "TenLoai"); 
    //     // -------------------------------------------
    //
    //     return View();
    // }
    
    // GET: Admin/BatDongSan/Create
    // public IActionResult Create()
    // {
    //     
    //     Console.WriteLine("--------------------------------------");
    //     Console.WriteLine("CHẠY VÀO HÀM CREATE RỒI NHÉ !!!!");
    //     Console.WriteLine("--------------------------------------");
    //     // 1. Tạo dữ liệu giả (Hard-code)
    //     var listTam = new List<LoaiBatDongSan> {
    //         new LoaiBatDongSan { Id = 1, TenLoai = "TEST: Chung cư" },
    //         new LoaiBatDongSan { Id = 2, TenLoai = "TEST: Nhà đất" }
    //     };
    //
    //     // 2. CHỖ QUAN TRỌNG NHẤT: Phải dùng đúng tên biến "listTam"
    //     // Sai lầm cũ: new SelectList(danhSachLoai, ...) -> Lỗi đỏ lòm
    //     ViewBag.DsLoai = new SelectList(listTam, "Id", "TenLoai");
    //
    //     return View();
    // }
    

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BatDongSan batDongSan, IFormFile? fileAnh)
    {
        if (ModelState.IsValid)
        {
            // 1. Xử lý ảnh (Nếu có chọn ảnh)
            if (fileAnh != null)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(fileAnh.FileName);
                string productPath = Path.Combine(wwwRootPath, @"images/products");

                if (!Directory.Exists(productPath)) Directory.CreateDirectory(productPath);

                using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                {
                    await fileAnh.CopyToAsync(fileStream);
                }
                
                // Cập nhật đường dẫn ảnh vào Model
                batDongSan.HinhAnh = @"/images/products/" + fileName;
            }
            
            // 2. Lưu vào Database (Logic này phải nằm ngoài khối if fileAnh)
            await _uow.BatDongSan.AddAsync(batDongSan);
            await _uow.SaveAsync();
            
            // 3. Thông báo thành công (Optional - để làm sau)
            return RedirectToAction(nameof(Index));
        }
        
        // --- XỬ LÝ KHI CÓ LỖI (VALIDATION ERROR) ---
        // Nếu code chạy xuống đây nghĩa là nhập liệu sai (ModelState.IsValid = false)
        // Cần load lại Dropdown để người dùng chọn lại, KHÔNG ĐƯỢC QUÊN
        
        var danhSachLoai = await _uow.LoaiBatDongSan.GetAllAsync();
        // SỬA LẠI TÊN BIẾN CHO KHỚP VỚI HÀM GET (DsLoai)
        ViewBag.DsLoai = new SelectList(danhSachLoai, "Id", "TenLoai");
        
        return View(batDongSan);
    }
}