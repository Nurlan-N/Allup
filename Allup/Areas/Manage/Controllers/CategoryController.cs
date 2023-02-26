using Allup.DataAccessLayer;
using Allup.Extentions;
using Allup.Helpers;
using Allup.Models;
using Allup.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace Allup.Areas.Manage.Controllers
{
    [Area("manage")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoryController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            IQueryable<Category> categories = _context.Categories.Include(c => c.Products)
                .Where(c => c.IsDeleted == false && c.IsMain);
            return View(PageNatedList<Category>.Create(categories,pageIndex,3));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null) { return BadRequest(); }
            Category category = await _context.Categories
                .Include(c => c.Children.Where(a => a.IsDeleted == false))
                .Include(c => c.Products.Where(p => p.IsDeleted == false))
                .FirstOrDefaultAsync(c => c.IsDeleted == false && c.Id == id);

            if (category == null) { return NotFound(); }

            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            ViewBag.Categories = await _context.Categories.Include(c => c.Products).Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(category);
            }
            if (await _context.Categories.AnyAsync(c => c.IsDeleted == false && c.Name.ToLower().Contains(category.Name.Trim().ToLower())))
            {
                ModelState.AddModelError("Name", "Bu adda category var");
                return View(category);
            }
            if (category.IsMain)
            {
                if(category.Name == null)
                {
                    ModelState.AddModelError("Name", "Ad Mecburidir ");
                    return View(category);
                }
                if (category.File == null)
                {
                    ModelState.AddModelError("File", "Sekil Mecburidir ");
                    return View(category);
                }
                if (category.File?.ContentType != "image/jpeg")
                {
                    ModelState.AddModelError("File", "File tipi uygun deyil");
                    return View(category);
                }
                if (category.File?.Length / 1024 > 500)
                {
                    ModelState.AddModelError("File", "File olcusu uygun deyil . Max 50kb");
                    return View(category);
                }

                category.Image = await category.File.CreateFileAsync(_webHostEnvironment,"assets","images");

                category.ParentId = null;
            }
            else
            {
                if (category.ParentId == null) 
                { 
                    ModelState.AddModelError("ParentId", "Ust kateqoria Sec !!!");
                    return View(category);

                }
                
                if (!await _context.Categories.AnyAsync(c => c.IsDeleted == false && c.IsMain && c.Id == category.ParentId))
                {
                    ModelState.AddModelError("ParentId", "Duzgun ust category sec");
                    return View(category);
                }

                category.Image = null;
            }

            category.Name = category.Name.Trim();
            category.CreatedAt = DateTime.UtcNow.AddHours(4);
            category.CreatedBy = "System";

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
           

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if(id== null) { return BadRequest(); }
            Category category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

            if (category == null) { return NotFound(); }

            ViewBag.Categories = await _context.Categories.Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();


            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id , Category category) 
        {
            ViewBag.Categories = await _context.Categories.Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();

            if(!ModelState.IsValid) 
            {
                return View(category);
            }
            if(id == null) { return BadRequest(); }
            if(id != category.Id) { return BadRequest(); }

            Category dbCategory = await _context.Categories.FirstOrDefaultAsync(c=> c.IsDeleted == false && c.Id== id);
            if (dbCategory == null) { return NotFound(); }

            if (category.IsMain)
            {
                if(dbCategory.IsMain)
                {
                    if(category.File != null)
                    {
                        if (category.File.CheckFileContentType("image/jpeg"))
                        {
                            ModelState.AddModelError("File", "File tipi uygun deyil");
                            return View(category);
                        }
                        if (!category.File.CheckFileLength(500))
                        {
                            ModelState.AddModelError("File", "File olcusu uygun deyil . Max 50kb");
                            return View(category);
                        }
                        FileHelpers.DeleteFile(dbCategory.Image, _webHostEnvironment, "assets", "images");

                        dbCategory.Image = await category.File.CreateFileAsync(_webHostEnvironment, "assets", "images");

                        
                    }

                }
                else
                {
                    if (category.File == null)
                    {
                        ModelState.AddModelError("File", "Sekil Mecburidir ");
                        return View(category);
                    }
                    if (category.File?.ContentType != "image/jpeg")
                    {
                        ModelState.AddModelError("File", "File tipi uygun deyil");
                        return View(category);
                    }
                    if (category.File?.Length / 1024 > 500)
                    {
                        ModelState.AddModelError("File", "File olcusu uygun deyil . Max 50kb");
                        return View(category);
                    }
                    category.Image = await category.File.CreateFileAsync(_webHostEnvironment, "assets", "images");                  
                }
                dbCategory.ParentId = null;
            }
            else
            {
                if (category.ParentId == null)
                {
                    ModelState.AddModelError("ParentId", "Ust kateqoria Sec !!!");
                    return View(category);
                }
                if (!await _context.Categories.AnyAsync(c => c.IsDeleted == false && c.IsMain && c.Id == category.ParentId))
                {
                    ModelState.AddModelError("ParentId", "Duzgun ust category sec");
                    return View(category);
                }
                if (category.Id == category.ParentId)
                {
                    ModelState.AddModelError("ParentId", "Eyni ola bilmez");
                    return View(category);
                }
                dbCategory.Image = null;
                dbCategory.ParentId = category.ParentId;
            }
            dbCategory.Name= category.Name.Trim();
            dbCategory.IsMain= category.IsMain;
            dbCategory.UpdatedBy = "System";
            dbCategory.CreatedAt= DateTime.UtcNow.AddHours(4);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            Category category = await _context.Categories
                .Include(c => c.Children.Where(a => a.IsDeleted == false))
                .Include(c => c.Products.Where(p => p.IsDeleted==false))
                .FirstOrDefaultAsync(c => c.IsDeleted == false && c.Id == id);

            if (category == null) return NotFound();

            return View(category);
        }
        [HttpGet]
        public async Task<IActionResult> DeleteCategory(int? id)
        {
            if (id == null) return BadRequest();

            Category category = await _context.Categories
                .Include(c => c.Children.Where(a => a.IsDeleted == false))
                .Include(c => c.Products.Where(p => p.IsDeleted == false))
                .FirstOrDefaultAsync(c => c.IsDeleted == false && c.Id == id);

            if (category == null) return NotFound();

            category.IsDeleted= true;
            category.DeletedAt= DateTime.UtcNow.AddHours(4);
            category.DeletedBy = "System";
            
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
