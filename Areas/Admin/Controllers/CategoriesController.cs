using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Winery.Models;
using Winery.Services;
using WineryDbContext = Winery.Data.DbContext;

namespace Winery.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly WineryDbContext _context;
        private readonly IFileService _fileService;

        public CategoriesController(WineryDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index()
        {
            // Eagerly load Products to avoid NullReferenceException in the View count badge
            var categories = await _context.Categories
                                       .Include(c => c.Products)
                                       .OrderBy(c => c.DisplayOrder)
                                       .ToListAsync(); 

            return View("~/Areas/Admin/Views/Category/Index.cshtml", categories);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View("~/Areas/Admin/Views/Category/Details.cshtml", category);
        }

        public IActionResult Create()
        {
            return View("~/Areas/Admin/Views/Category/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (category.CategoryImage != null)
                    {
                        category.ImageUrl = await _fileService.SaveFileAsync(
                            category.CategoryImage,
                            "images/category");
                    }

                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Category created successfully.";
                    return RedirectToAction(nameof(Index));
                }

                return View("~/Areas/Admin/Views/Category/Create.cshtml", category);
            }
            catch (Exception)
            {
                TempData["Error"] = "The category could not be created.";
                return View("~/Areas/Admin/Views/Category/Create.cshtml", category);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View("~/Areas/Admin/Views/Category/Edit.cshtml", category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            try
            {
                if (id != category.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    var existingCategory = await _context.Categories.FindAsync(id);
                    if (existingCategory == null)
                    {
                        return NotFound();
                    }

                    existingCategory.Name = category.Name;
                    existingCategory.Description = category.Description;
                    existingCategory.DisplayOrder = category.DisplayOrder;
                    existingCategory.IsActive = category.IsActive;

                    if (category.CategoryImage != null)
                    {
                        if (!string.IsNullOrWhiteSpace(existingCategory.ImageUrl))
                        {
                            _fileService.DeleteFile(existingCategory.ImageUrl);
                        }

                        existingCategory.ImageUrl = await _fileService.SaveFileAsync(
                            category.CategoryImage,
                            "images/category");
                    }

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Category updated successfully.";
                    return RedirectToAction(nameof(Index));
                }

                return View("~/Areas/Admin/Views/Category/Edit.cshtml", category);
            }
            catch (Exception)
            {
                TempData["Error"] = "The category could not be updated.";
                return View("~/Areas/Admin/Views/Category/Edit.cshtml", category);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View("~/Areas/Admin/Views/Category/Delete.cshtml", category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);

                if (category == null)
                {
                    return NotFound();
                }

                var imageUrl = category.ImageUrl;

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(imageUrl))
                {
                    _fileService.DeleteFile(imageUrl);
                }

                TempData["Success"] = "Category deleted successfully.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "The category could not be deleted because it is still in use.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
