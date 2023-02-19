using Allup.DataAccessLayer;
using Allup.Models;
using Microsoft.AspNetCore.Mvc;

namespace Allup.ViewComponents
{
    public class ProductViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public ProductViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult>  InvokeAsync()
        {
            IEnumerable<Product> products = _context.Products.Where(p => p.IsDeleted == false);

            return View(await Task.FromResult(products));
        }
    }
}
