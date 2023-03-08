using Microsoft.AspNetCore.Identity;
using WarehouseManager.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarehouseManager.Models.ViewModels;

namespace WarehouseManager.Controllers
{
    public class AdminController : Controller
    {
        private UserManager<User> userManager;
        private RoleManager<Role> roleManager;

        public AdminController(UserManager<User> userMngr, RoleManager<Role> roleMngr)
        {
            userManager = userMngr;
            roleManager = roleMngr;
        }

        public async Task<IActionResult> Index()
        {
            List<User> users = new List<User>();
            foreach (User user in userManager.Users)
            {
                user.RoleNames = await userManager.GetRolesAsync(user);
                users.Add(user);
            }
            UserViewModel model = new UserViewModel
            {
                Users = users,
                Roles = roleManager.Roles
            };
            return View(model);
        }
       
        public async Task<IActionResult> AddToRole(UserViewModel model, string id)
        {
            Role role = await roleManager.FindByNameAsync(model.RoleName);
            User user = await userManager.FindByIdAsync(id);
            await userManager.AddToRoleAsync(user, role.Name);
            return RedirectToAction("Index");
        }
        
    }
}
