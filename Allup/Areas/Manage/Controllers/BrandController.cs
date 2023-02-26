using Allup.DataAccessLayer;
using Allup.Models;
using Allup.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Allup.Areas.Manage.Controllers
{
    [Area("manage")]
    public class BrandController : Controller
    {
        private readonly AppDbContext _context;

        public BrandController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            IQueryable<Brand> brands = _context.Brands
                .Include(b => b.Products.Where(p => p.IsDeleted == false))
                .Where(b => b.IsDeleted == false)
                .OrderByDescending(b => b.Id);

            return View(PageNatedList<Brand>.Create(brands,pageIndex,3));
        }

        [HttpGet]
        public IActionResult Create()
        {

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Brand  brand)
        {
            if (!ModelState.IsValid) 
            { 
                return View(brand);
            }
            if (await _context.Brands.AnyAsync(b => b.IsDeleted == false && b.Name.ToLower().Contains(brand.Name.Trim().ToLower())))
            {
                ModelState.AddModelError("Name", "Bu Adda Brand Artiq Movcuddur");

                return View(brand);
            }

            brand.Name = brand.Name.Trim();
            brand.CreatedBy = "System";
            brand.CreatedAt= DateTime.UtcNow.AddHours(4);

            await _context.Brands.AddAsync(brand);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null){return BadRequest();}

            Brand brand = await _context.Brands.Include(b => b.Products.Where(b => b.IsDeleted == false)).FirstOrDefaultAsync(b => b.IsDeleted == false && b.Id == id);
            if (brand == null) { return NotFound(); }

            return View(brand);
        }
        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Brand brand = await _context.Brands.FirstOrDefaultAsync(b => b.IsDeleted == false &&  b.Id == id);
            if (brand == null) { return NotFound(); }

            return View(brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, Brand brand)
        {
            if (!ModelState.IsValid) { return View(brand); }
            if (id == null){return BadRequest();}
            if (id != brand.Id) { return BadRequest();}

            Brand dbBrand = await _context.Brands.FirstOrDefaultAsync(b => b.IsDeleted == false && b.Id == id);

            if (brand == null) { return NotFound(); }
            if (await _context.Brands.AnyAsync(b => b.IsDeleted == false && b.Name.ToLower().Contains(brand.Name.Trim().ToLower()) && brand.Id != b.Id))
            {
                ModelState.AddModelError("Name", " Bu Adda Brand Artiq Movcuddur");

                return View(brand);
            }
            dbBrand.Name = brand.Name;
            dbBrand.UpdatedBy = brand.UpdatedBy;
            dbBrand.UpdatedAt= brand.UpdatedAt;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) { return BadRequest(); }

            Brand brand = await _context.Brands.Include(b => b.Products.Where(b => b.IsDeleted == false)).FirstOrDefaultAsync(b => b.IsDeleted == false && b.Id == id);
            if (brand == null) { return NotFound(); }

            return View(brand);
        }
    }
}
