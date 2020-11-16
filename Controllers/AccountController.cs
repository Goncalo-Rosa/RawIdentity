using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RawIdentity.Data;
using RawIdentity.Models;
using RawIdentity.Models.ViewModels;


namespace RawIdentity
{
    [Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        private readonly MemberShipDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;


        public AccountController(MemberShipDbContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ListAllUsers()
        {
            var model = _userManager.Users;

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View("Login");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register([Bind("Email,Password,ConfirmPassword")] RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([Bind("Email,Password,RememberMe")] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Login Failed");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            ViewBag.UserRoles = roles;

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Remove(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);

            if (user != null)
            {
                return View(user);
            }

            return RedirectToAction("ListAllUsers", "Account");
        }

        [HttpPost, ActionName("Remove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveConfirmed(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                TempData["RemoveSuccess"] = user.UserName + "removed successfully!";
                return RedirectToAction("ListAllUsers", "Account");
            }

            foreach (var item in result.Errors)
            {
                ModelState.AddModelError("", item.Description);
            }

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles(string Id)
        {

            var user = await _userManager.FindByIdAsync(Id);

            if (user == null)
            {
                return NotFound();
            }

            var listModel = new List<ManageUserRolesViewModel>();

            foreach (var item in _roleManager.Roles.ToList())
            {
                var model = new ManageUserRolesViewModel
                {
                    RoleId = item.Id,
                    RoleName = item.Name
                };

                model.IsSelected = await _userManager.IsInRoleAsync(user, item.Name) ? true : false;

                listModel.Add(model);
            }

            ViewBag.UserId = user.Id;
            ViewBag.UserName = user.UserName;

            return View(listModel);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUsersRoles(string Id, List<ManageUserRolesViewModel> model)
        {

            var user = await _userManager.FindByIdAsync(Id);

            if (user == null)
            {
                return NotFound();
            }

            foreach (var item in model)
            {
                var role = await _roleManager.FindByIdAsync(item.RoleId);

                if (item.IsSelected && !await _userManager.IsInRoleAsync(user, role.Name))
                {

                    await _userManager.AddToRoleAsync(user, role.Name);

                }
                else if (!item.IsSelected && await _userManager.IsInRoleAsync(user, role.Name))
                {

                    await _userManager.RemoveFromRoleAsync(user, role.Name);

                }
            }

            return RedirectToAction("ListAllUsers", "Account");
        }
    }
}