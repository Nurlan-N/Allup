using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Allup.Models
{
    public class AppUser : IdentityUser
    {
        [StringLength(20)]
        public string? Name { get; set; }
        [StringLength(20)]
        public string? SurName { get; set; }
        [StringLength(20)]
        public string? FatherName { get; set; }
        public string? ProfilImage { get; set; }

    }
}
