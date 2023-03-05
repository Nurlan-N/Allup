using System.ComponentModel.DataAnnotations;

namespace Allup.Areas.Manage.ViewModels.AccountVMs
{
    public class ProfileVM
    {
        [StringLength(20)]
        public string? Name { get; set; }
        [StringLength(20)]
        public string? SurName { get; set; }
        [StringLength(20)]
        public string? FatherName { get; set; }
        public string? ProfilImage { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string UserName { get; set; }
        [StringLength(20)]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }
        [StringLength(20)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [StringLength(20)]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirimPassword { get; set; }
    }
}
