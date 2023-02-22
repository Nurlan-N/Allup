using Allup.DataAccessLayer;
using Allup.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IActionResult> Index()
        {

            return View(await _context.Categories.Include(c => c.Products).Where(c => c.IsDeleted == false && c.IsMain).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.Include(c => c.Products).Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();

            return View();
        }
        [HttpPost]
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

                string fileName = $"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}-{Guid.NewGuid().ToString()}-{category.File.FileName}";

                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "images", fileName);

                category.Image = fileName;
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    await category.File.CopyToAsync(stream);
                }

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
    }
}
