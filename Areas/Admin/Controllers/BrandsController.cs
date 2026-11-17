using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Winery.Models;
using Winery.Services;
using WineryDbContext = Winery.Data.DbContext;

namespace Winery.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class BrandsController : Controller
{
    private readonly WineryDbContext _context;
    private readonly IFileService _fileService;

    public BrandsController(
        WineryDbContext context,
        IFileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    // GET: BRANDS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Brands.Include(c => c.Products)
             .ToListAsync());
    }



    // GET: BRANDS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: BRANDS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create( Brand brand)
    {
        try
        {
            if (ModelState.IsValid)
            {
                if (brand.BrandLogo != null)
                {
                    brand.LogoUrl = await _fileService.SaveFileAsync(
                        brand.BrandLogo,
                        "images/brand");
                }

                _context.Brands.Add(brand);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Brand added successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(brand);
        }
        catch (Exception)
        {
            TempData["Error"] = "The brand could not be added.";
            return View(brand);
        }
    }

    // GET: BRANDS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var brand = await _context.Brands.FindAsync(id);
        if (brand == null)
        {
            return NotFound();
        }
        return View(brand);
    }

    // POST: BRANDS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Brand brand)
    {
        try
        {
            if (id != brand.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(brand);
            }

            var existingBrand = await _context.Brands.FindAsync(id);

            if (existingBrand == null)
            {
                return NotFound();
            }

            existingBrand.Name = brand.Name;
            existingBrand.Description = brand.Description;
            existingBrand.Country = brand.Country;
            existingBrand.WebsiteUrl = brand.WebsiteUrl;
            existingBrand.IsActive = brand.IsActive;

            if (brand.BrandLogo != null)
            {
                if (!string.IsNullOrWhiteSpace(existingBrand.LogoUrl))
                {
                    _fileService.DeleteFile(existingBrand.LogoUrl);
                }

                existingBrand.LogoUrl = await _fileService.SaveFileAsync(
                    brand.BrandLogo,
                    "images/brand");
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Brand updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            TempData["Error"] = "The brand could not be updated.";
            return View(brand);
        }
    }

    // GET: BRANDS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var brand = await _context.Brands
            .FirstOrDefaultAsync(m => m.Id == id);
        if (brand == null)
        {
            return NotFound();
        }

        return View(brand);
    }

    // POST: BRANDS/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }

            brand.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Brand deleted successfully.";
        }
        catch (Exception)
        {
            TempData["Error"] = "The brand could not be deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool BrandExists(int? id)
    {
        return _context.Brands.Any(e => e.Id == id);
    }
}
