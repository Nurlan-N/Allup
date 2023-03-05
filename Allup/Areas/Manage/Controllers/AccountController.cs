using Allup.Areas.Manage.ViewModels.AccountVMs;
using Allup.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Allup.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid) { return View(); }

            AppUser appUser = new AppUser
            { 
                Name = registerVM.Name,
                Email = registerVM.Email,
                FatherName = registerVM.FatherName,
                SurName = registerVM.SurName,
                UserName = registerVM.UserName,
            };

            IdentityResult identityResult = await _userManager.CreateAsync(appUser,registerVM.Password);
            if (!identityResult.Succeeded) 
            {
                foreach (IdentityError identityError in identityResult.Errors )
                {
                    ModelState.AddModelError("", identityError.Description);
                }
                return View(registerVM);
            }
            await _userManager.AddToRoleAsync(appUser, "Admin");

            return RedirectToAction("Index", "dashboard", new {area = "manage"});
        }

        [HttpGet]
        public  IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid) { return View(loginVM); }
            
            AppUser appUser = await _userManager.FindByEmailAsync(loginVM.Email);
            if (appUser == null) 
            {
                ModelState.AddModelError("", "Email veya Password Yalnisdir");
                return View(loginVM);
            }
            Microsoft.AspNetCore.Identity.SignInResult signInResult = await _signInManager.PasswordSignInAsync(appUser, loginVM.Password, loginVM.RememberMe, true);
            if (!signInResult.Succeeded) 
            {
                ModelState.AddModelError("", "Email veya Password Yalnisdir");
                return View(loginVM);
            }

            return RedirectToAction("index", "dashboard", new { areas = "manage" });
         }
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("login");
        }
        [HttpGet]
        [Authorize] 
        public async Task<IActionResult> Profile()
        {
            AppUser appUser = await _userManager.FindByNameAsync(User.Identity.Name);

            ProfileVM profileVM = new ProfileVM
            {
                Name = appUser.Name,
                Email = appUser.Email,
                SurName = appUser.SurName,
                UserName = appUser.UserName,
                FatherName = appUser.FatherName,

            };

            return View(profileVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileVM profileVM)
        {
            if(!ModelState.IsValid) { return View(profileVM);}

            AppUser appUser = await _userManager.FindByNameAsync(User.Identity.Name);

            appUser.Name = profileVM.Name;
            appUser.SurName = profileVM.SurName;
            appUser.FatherName = profileVM.FatherName;

            if (appUser.NormalizedEmail != profileVM.Email.Trim().ToUpperInvariant())
            {
                appUser.Email = profileVM.Email;
            }
            if (appUser.NormalizedUserName != profileVM.UserName.Trim().ToUpperInvariant())
            {
                appUser.UserName = profileVM.UserName;
            }

            IdentityResult identityResult = await _userManager.UpdateAsync(appUser);

            if (!identityResult.Succeeded) 
            {
                foreach (IdentityError identityError in identityResult.Errors)
                {
                    ModelState.AddModelError("", identityError.Description);
                }

                return View(profileVM);
            }
            await _signInManager.SignInAsync(appUser,true);
            if (!string.IsNullOrWhiteSpace(profileVM.Password))
            {
                if (!await _userManager.CheckPasswordAsync(appUser,profileVM.OldPassword))
                {
                    ModelState.AddModelError("OldPassword", "Kohne sifre yalnisdir");
                    return View(profileVM);
                }
                string token = await _userManager.GeneratePasswordResetTokenAsync(appUser);

                identityResult = await _userManager.ResetPasswordAsync(appUser, token, profileVM.Password);
                if (!identityResult.Succeeded)
                {
                    foreach (IdentityError identityError in identityResult.Errors)
                    {
                        ModelState.AddModelError("", identityError.Description);
                    }

                    return View(profileVM);
                }
            }

            


            return RedirectToAction("index", "dashboard", new { area = "manage" });
        }

        #region Create Super Admin
        //[HttpGet]
        //public async Task<IActionResult> CreateRole()
        //{
        //    await _roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
        //    await _roleManager.CreateAsync(new IdentityRole("Admin"));
        //    await _roleManager.CreateAsync(new IdentityRole("Menber"));

        //    return Content("Ugurlu Oldu");
        //}

        //[HttpGet]
        //public async Task<IActionResult> CreateUser()
        //{
        //    AppUser appUser = new AppUser
        //    {
        //        Name = "Nurlan",
        //        SurName = "Nazarov",
        //        FatherName = "Bahadur",
        //        UserName = "SuperAdmin",
        //        Email = "Nazarov.Nurlan@gmail.com"
        //    };

        //    await _userManager.CreateAsync(appUser, "SuperAdmin.434");
        //    await _userManager.AddToRoleAsync(appUser, "SuperAdmin");

        //    return Content("Ugurlu Oldu");

        //}
        #endregion
    }
}
