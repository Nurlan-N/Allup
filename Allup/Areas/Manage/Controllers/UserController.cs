using Allup.Areas.Manage.ViewModels.UserVMs;
using Allup.DataAccessLayer;
using Allup.Models;
using Allup.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Allup.Areas.Manage.Controllers
{

    [Area("manage")]
    [Authorize(Roles = "SuperAdmin")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<AppUser> userManager, AppDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            List<UserVM> query = await  _userManager.Users.Where(u => u.UserName != User.Identity.Name)
                .Select(x => new UserVM
                {
                    Id = x.Id,
                    Name = x.Name,
                    SurName = x.SurName,
                    Email = x.Email,
                    UserName = x.UserName
                })
            .ToListAsync();

            foreach (var item in query)
            {
                string roleId = _context.UserRoles.FirstOrDefault(u => u.UserId == item.Id).RoleId;
                string roleName = _context.Roles.FirstOrDefault(r => r.Id == roleId).Name;
                
                item.RoleName = roleName;
            }
            return View(PageNatedList<UserVM>.Create(query.AsQueryable(),pageIndex,3));
        }
        [HttpGet]
        public async Task<IActionResult> ChangeRole(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            AppUser appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null) { return NotFound(); }

            string roleId = _context.UserRoles.FirstOrDefault(u => u.UserId == appUser.Id).RoleId;

            UserChangeRoleVM userChangeRoleVM = new UserChangeRoleVM
            {
                UserId = appUser.Id,
                RoleId = roleId,
            };

            ViewBag.Role = await _roleManager.Roles.Where(c => c.Name != "SuperAdmin").ToListAsync();

            return View(userChangeRoleVM);
        }
        [HttpPost]
        public async Task<IActionResult> ChangeRole(UserChangeRoleVM userChangeRoleVM)
        {
            ViewBag.Role = await _roleManager.Roles.Where(c => c.Name != "SuperAdmin").ToListAsync();

            if (!ModelState.IsValid) { return View(userChangeRoleVM); }


            AppUser appUser = await _userManager.FindByIdAsync(userChangeRoleVM.UserId);

            if (appUser == null) { return NotFound(); }

            string roleId = _context.UserRoles.FirstOrDefault(u => u.UserId == userChangeRoleVM.UserId).RoleId;
            string roleName = _context.Roles.FirstOrDefault(r => r.Id == roleId).Name;
            string newRoleName = _roleManager.Roles.FirstOrDefault(c => c.Name != "SuperAdmin" && c.Id == userChangeRoleVM.RoleId).Name;

            await _userManager.RemoveFromRoleAsync(appUser, roleName);
            await _userManager.AddToRoleAsync(appUser, newRoleName);
 
            return RedirectToAction("Index");
        }
    }   
}
