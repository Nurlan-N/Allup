using Allup.DataAccessLayer;
using Allup.Models;
using Allup.ViewModels.ProductViewModel;
using Microsoft.AspNetCore.Mvc;

namespace Allup.ViewComponents
{
    public class ProductViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult>  InvokeAsync(ProductVM productVM)
        {
            return View(await Task.FromResult(productVM));
        }
    }
}
