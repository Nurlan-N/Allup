using Allup.Models;

namespace Allup.ViewModels.HomeViewModels
{
    public class HomeVM
    {
        public IEnumerable<Slider> Sliders { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Product> FeaturesProducts { get; set; }
        public IEnumerable<Product> BestSellerProducts { get; set; }
        public IEnumerable<Product> NewArrivalProducts { get; set; }


    }
}
