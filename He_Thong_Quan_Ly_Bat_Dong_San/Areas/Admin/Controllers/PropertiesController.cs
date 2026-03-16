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

        // =========================
        // INDEX + PAGINATION
        // =========================
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 5;

            var propertiesQuery = _context.Properties
                .Include(p => p.Category)
                .AsQueryable();

            int totalItems = await propertiesQuery.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var properties = await propertiesQuery
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(properties);
        }

        // =========================
        // DETAILS
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var property = await _context.Properties
                .Include(p => p.Category)
                .Include(p => p.PropertyImages)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (property == null) return NotFound();

            return View(property);
        }

        // =========================
        // CREATE
        // =========================
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Property property)
        {
            if (ModelState.IsValid)
            {
                // ===== UPLOAD ẢNH ĐẠI DIỆN =====
                if (property.ImageUpload != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid() + "_" + property.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await property.ImageUpload.CopyToAsync(fileStream);
                    }

                    property.ImageUrl = "/uploads/" + uniqueFileName;
                }
                else
                {
                    property.ImageUrl = "/images/default-property.jpg";
                }

                // ===== LƯU PROPERTY TRƯỚC =====
                _context.Add(property);
                await _context.SaveChangesAsync();


                // ===== LƯU GALLERY ẢNH =====
                if (property.GalleryUploads != null && property.GalleryUploads.Count > 0)
                {
                    var uploads = property.GalleryUploads.Take(5).ToList();

                    string galleryFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "gallery");

                    if (!Directory.Exists(galleryFolder))
                        Directory.CreateDirectory(galleryFolder);

                    foreach (var file in uploads)
                    {
                        string uniqueFileName = Guid.NewGuid() + "_" + file.FileName;
                        string filePath = Path.Combine(galleryFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        var propImage = new PropertyImage
                        {
                            PropertyId = property.Id,
                            ImageUrl = "/uploads/gallery/" + uniqueFileName
                        };

                        _context.PropertyImages.Add(propImage);
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", property.CategoryId);
            return View(property);
        }

        // =========================
        // EDIT
        // =========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var property = await _context.Properties.FindAsync(id);
            if (property == null) return NotFound();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", property.CategoryId);
            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Property property)
        {
            if (id != property.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProperty = await _context.Properties
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == id);

                    if (property.ImageUpload != null)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                        string uniqueFileName = Guid.NewGuid() + "_" + property.ImageUpload.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await property.ImageUpload.CopyToAsync(fileStream);
                        }

                        property.ImageUrl = "/uploads/" + uniqueFileName;

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
                        property.ImageUrl = existingProperty.ImageUrl;
                    }

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

        // =========================
        // DELETE
        // =========================
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

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var property = await _context.Properties.FindAsync(id);

            if (property != null)
            {
                if (property.ImageUrl != null)
                {
                    string imagePath = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        property.ImageUrl.TrimStart('/')
                    );

                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }

                _context.Properties.Remove(property);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PropertyExists(int id)
        {
            return _context.Properties.Any(e => e.Id == id);
        }
    }
}