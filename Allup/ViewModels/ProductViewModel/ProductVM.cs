using Allup.Models;

namespace Allup.ViewModels.ProductViewModel
{
    public class ProductVM
    {
        public IEnumerable<Product> FeaturesProducts { get; set; }
        public IEnumerable<Product> BestSellerProducts { get; set; }
        public IEnumerable<Product> NewArrivalProducts { get; set; }
    }
}
