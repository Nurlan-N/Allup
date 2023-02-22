using System.ComponentModel.DataAnnotations;

namespace Allup.Models
{
    public class Brand :BaseEntity
    {
        [StringLength(255,ErrorMessage ="Maksimum 255 simvol")]
        [Required(ErrorMessage = "Mecburidir ")]
        public string Name { get; set; }
        public IEnumerable<Product>? Products { get; set; }
    }
}
