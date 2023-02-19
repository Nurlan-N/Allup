using Allup.DataAccessLayer;
using Allup.Models;
using Microsoft.AspNetCore.Mvc;

namespace Allup.ViewComponents
{
    public class CategoryViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public CategoryViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            IEnumerable<Category> categories = _context.Categories.Where(c => c.IsDeleted == false);

            return View(await Task.FromResult(categories));
        }
    }
}
