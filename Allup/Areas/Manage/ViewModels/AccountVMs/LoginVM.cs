using System.ComponentModel.DataAnnotations;

namespace Allup.Areas.Manage.ViewModels.AccountVMs
{
    public class LoginVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required, StringLength(20)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
