using Allup.DataAccessLayer;
using Allup.ViewModels.HomeViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Allup.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            HomeVM vm = new HomeVM
            {
                Sliders = await _context.Sliders.Where(s => s.IsDeleted == false).ToListAsync(),
                Categories = await _context.Categories.Where(c => c.IsDeleted== false && c.IsMain == true).ToListAsync(),
                FeaturesProducts = await _context.Products.Where(p => p.IsDeleted == false && p.IsFeatured == true).ToListAsync(),
                BestSellerProducts = await _context.Products.Where(p => p.IsDeleted == false && p.IsBestSeller == true).ToListAsync(),
                NewArrivalProducts = await _context.Products.Where(p => p.IsDeleted == false && p.IsNewArrival == true).ToListAsync(),


            };
            return View(vm);
        }

        //public async Task<IActionResult> SetSession()
        //{
        //    HttpContext.Session.SetString("P133", "First Session Data");

        //    return Content("Session elave olundu");
        //}

        //public async Task<IActionResult> GetSession()
        //{

        //    var ses = HttpContext.Session.GetString("P133");

        //    return Content(ses);
        //}
    }
}
