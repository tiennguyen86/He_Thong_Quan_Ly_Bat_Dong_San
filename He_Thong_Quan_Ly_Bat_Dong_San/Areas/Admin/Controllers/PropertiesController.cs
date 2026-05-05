using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using He_Thong_Quan_Ly_Bat_Dong_San.Data;
using He_Thong_Quan_Ly_Bat_Dong_San.Models;

namespace He_Thong_Quan_Ly_Bat_Dong_San.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller CRUD quản lý bất động sản (Property)
    /// Cho phép Admin, Sale tạo/sửa/xem BĐS, upload ảnh đại diện và gallery
    /// Chỉ Admin được xóa BĐS
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Admin, Sale")]
    public class PropertiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PropertiesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// GET: Admin/Properties/Index
        /// Hiển thị danh sách bất động sản với phân trang
        /// </summary>
        /// <param name="page">Số trang hiện tại (mặc định: 1)</param>
        /// <returns>View danh sách BĐS</returns>
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 5;                                      // 5 BĐS trên 1 trang

            var propertiesQuery = _context.Properties
                .Include(p => p.Category)
                .AsQueryable();

            int totalItems = await propertiesQuery.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var properties = await propertiesQuery
                .OrderByDescending(p => p.Id)                      // Mới nhất lên đầu
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(properties);
        }

        /// <summary>
        /// GET: Admin/Properties/Details/{id}
        /// Hiển thị chi tiết một BĐS (bao gồm gallery ảnh)
        /// </summary>
        /// <param name="id">Mã BĐS cần xem</param>
        /// <returns>View chi tiết BĐS, hoặc 404 nếu không tìm thấy</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var property = await _context.Properties
                .Include(p => p.Category)
                .Include(p => p.PropertyImages)                   // Kéo theo danh sách ảnh
                .FirstOrDefaultAsync(m => m.Id == id);

            if (property == null) return NotFound();

            return View(property);
        }

        /// <summary>
        /// GET: Admin/Properties/Create
        /// Hiển thị form tạo BĐS mới (có upload ảnh đại diện + gallery)
        /// </summary>
        /// <returns>View form tạo BĐS</returns>
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        /// <summary>
        /// POST: Admin/Properties/Create
        /// Xử lý form tạo BĐS mới, bao gồm upload ảnh
        /// </summary>
        /// <param name="property">Dữ liệu BĐS từ form (Title, Price, Area, Address, Description, CategoryId, ImageUpload, GalleryUploads)</param>
        /// <returns>
        /// - Nếu thành công: Redirect về Index
        /// - Nếu lỗi validation: Hiển thị lại form
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Property property)
        {
            if (ModelState.IsValid)
            {
                // ===== BƯỚC 1: UPLOAD ẢNH ĐẠI DIỆN =====
                if (property.ImageUpload != null)
                {
                    // Tạo thư mục uploads nếu chưa có
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    // Tạo tên file duy nhất (dùng GUID để tránh trùng lặp)
                    string uniqueFileName = Guid.NewGuid() + "_" + property.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Lưu file lên server
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await property.ImageUpload.CopyToAsync(fileStream);
                    }

                    // Lưu đường dẫn vào DB
                    property.ImageUrl = "/uploads/" + uniqueFileName;
                }
                else
                {
                    // Nếu không chọn ảnh, dùng ảnh mặc định
                    property.ImageUrl = "/images/default-property.jpg";
                }

                // ===== BƯỚC 2: LƯU PROPERTY VÀO DB TRƯỚC =====
                // (để lấy ID để gắn vào gallery)
                _context.Add(property);
                await _context.SaveChangesAsync();


                // ===== BƯỚC 3: UPLOAD GALLERY ẢNH (tối đa 5 ảnh) =====
                if (property.GalleryUploads != null && property.GalleryUploads.Count > 0)
                {
                    // Lấy tối đa 5 ảnh
                    var uploads = property.GalleryUploads.Take(5).ToList();

                    string galleryFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "gallery");

                    // Tạo thư mục gallery nếu chưa có
                    if (!Directory.Exists(galleryFolder))
                        Directory.CreateDirectory(galleryFolder);

                    // Xử lý từng ảnh gallery
                    foreach (var file in uploads)
                    {
                        // Tạo tên file duy nhất
                        string uniqueFileName = Guid.NewGuid() + "_" + file.FileName;
                        string filePath = Path.Combine(galleryFolder, uniqueFileName);

                        // Lưu file lên server
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        // Tạo record PropertyImage trong DB
                        var propImage = new PropertyImage
                        {
                            PropertyId = property.Id,                          // Liên kết tới BĐS vừa tạo
                            ImageUrl = "/uploads/gallery/" + uniqueFileName
                        };

                        _context.PropertyImages.Add(propImage);
                    }

                    // Lưu tất cả PropertyImage vào DB
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", property.CategoryId);
            return View(property);
        }

        /// <summary>
        /// GET: Admin/Properties/Edit/{id}
        /// Hiển thị form sửa thông tin BĐS
        /// </summary>
        /// <param name="id">Mã BĐS cần sửa</param>
        /// <returns>View form sửa BĐS, hoặc 404 nếu không tìm thấy</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var property = await _context.Properties.FindAsync(id);
            if (property == null) return NotFound();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", property.CategoryId);
            return View(property);
        }

        /// <summary>
        /// POST: Admin/Properties/Edit/{id}
        /// Xử lý form sửa thông tin BĐS (bao gồm upload ảnh mới nếu có)
        /// </summary>
        /// <param name="id">Mã BĐS cần sửa</param>
        /// <param name="property">Dữ liệu BĐS đã sửa từ form</param>
        /// <returns>
        /// - Nếu thành công: Redirect về Index
        /// - Nếu lỗi concurrency: Thông báo lỗi
        /// - Nếu lỗi validation: Hiển thị lại form
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Property property)
        {
            if (id != property.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy thông tin BĐS cũ từ DB (để xóa ảnh cũ nếu có upload ảnh mới)
                    var existingProperty = await _context.Properties
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == id);

                    // Nếu có upload ảnh mới
                    if (property.ImageUpload != null)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                        // Tạo tên file duy nhất và lưu
                        string uniqueFileName = Guid.NewGuid() + "_" + property.ImageUpload.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await property.ImageUpload.CopyToAsync(fileStream);
                        }

                        property.ImageUrl = "/uploads/" + uniqueFileName;

                        // Xóa ảnh cũ nếu tồn tại
                        if (existingProperty.ImageUrl != null)
                        {
                            string oldImagePath = Path.Combine(
                                _webHostEnvironment.WebRootPath,
                                existingProperty.ImageUrl.TrimStart('/')
                            );

                            if (System.IO.File.Exists(oldImagePath))
                                System.IO.File.Delete(oldImagePath);
                        }
                    }
                    else
                    {
                        // Nếu không upload ảnh mới, giữ nguyên ảnh cũ
                        property.ImageUrl = existingProperty.ImageUrl;
                    }

                    // Cập nhật BĐS vào DB
                    _context.Update(property);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PropertyExists(property.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", property.CategoryId);
            return View(property);
        }

        /// <summary>
        /// GET: Admin/Properties/Delete/{id}
        /// Hiển thị trang xác nhận xóa BĐS
        /// CẢNH BÁO: Chỉ Admin được xóa
        /// </summary>
        /// <param name="id">Mã BĐS cần xóa</param>
        /// <returns>View xác nhận xóa, hoặc 404 nếu không tìm thấy</returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var property = await _context.Properties
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (property == null) return NotFound();

            return View(property);
        }

        /// <summary>
        /// POST: Admin/Properties/Delete/{id}
        /// Xử lý xóa BĐS (bao gồm xóa file ảnh trên server)
        /// Chỉ Admin được thực hiện
        /// </summary>
        /// <param name="id">Mã BĐS cần xóa</param>
        /// <returns>Redirect về Index</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var property = await _context.Properties.FindAsync(id);

            if (property != null)
            {
                // Xóa ảnh đại diện nếu tồn tại
                if (property.ImageUrl != null)
                {
                    string imagePath = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        property.ImageUrl.TrimStart('/')
                    );

                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }

                // Xóa BĐS khỏi DB
                _context.Properties.Remove(property);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Phương thức hỗ trợ kiểm tra BĐS có tồn tại không
        /// </summary>
        /// <param name="id">Mã BĐS cần kiểm tra</param>
        /// <returns>True nếu tồn tại, False nếu không</returns>
        private bool PropertyExists(int id)
        {
            return _context.Properties.Any(e => e.Id == id);
        }
    }
}