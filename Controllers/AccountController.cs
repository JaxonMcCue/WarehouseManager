using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarehouseManager.Data;
using WarehouseManager.Models;
using WarehouseManager.Models.ViewModels;

namespace WarehouseManager.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<User> userManager;
        private SignInManager<User> signInManager;
        private readonly ApplicationDbContext _context;


        public AccountController(UserManager<User> usrMgr, SignInManager<User> signInMgr, ApplicationDbContext context)
        {
            userManager = usrMgr;
            signInManager = signInMgr;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> MyAccount()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            if (user.CustomerID != null)
            {
                var customer =  _context.Customers.FirstOrDefault(c => c.CustomerID == user.CustomerID);
            }
            return View(user);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = new Customer
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Address = model.Address,
                    City = model.City,
                    State = model.State,
                    Zip = model.Zip,
                    Phone = model.Phone
                };
                _context.Customers.Add(customer);
                _context.SaveChanges();


                var user = new User { UserName = model.UserName, Email = model.Email, CustomerID = customer.CustomerID };
                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult LogIn(string returnURL = "")
        {
            var model = new LoginViewModel { ReturnUrl = returnURL };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(
                    model.Username, model.Password, isPersistent: model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            ModelState.AddModelError("", "Invalid username/password");
            return View(model);
        }

        public ViewResult AccessDenied()
        {
            return View();
        }

        //[HttpGet]
        //public IActionResult ChangePassword()
        //{
        //    var model = new ChangePasswordViewModel
        //    {
        //        Username = User.Identity?.Name ?? ""
        //    };
        //    return View(model);
        //}
        //[HttpGet]
        
        //public IActionResult ChangeEmail()
        //{
        //    var model = new ChangeEmailViewModel
        //    {
        //        Username = User.Identity?.Name ?? ""
        //    };
        //    return View(model);
        //}

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var username = User.Identity.Name;
                User user = await userManager.FindByNameAsync(username);
                var result = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    TempData["message"] = "Password Changed Successfully";
                    return RedirectToAction("Manage");
                }
                else
                {
                    TempData["message"] = "Error";
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return RedirectToAction("Manage");
        }
        
        [HttpPost]
        public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var username = User.Identity.Name;
                User user = await userManager.FindByNameAsync(username);
                var passwordCorrect = await userManager.CheckPasswordAsync(user, model.ConfirmPassword);
                if (!passwordCorrect)
                {
                    TempData["error"] = "The password is incorrect.";
                    return RedirectToAction("Manage");
                }
                var email = await userManager.GenerateChangeEmailTokenAsync(user, model.NewEmail);
                var result = await userManager.ChangeEmailAsync(user, model.NewEmail, email);

                if (result.Succeeded)
                {
                    TempData["message"] = "Email Changed Successfully";
                    return RedirectToAction("Manage");
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
                // create a new EditUserViewModel object and pass it to the view
                var editUser = new UserEditViewModel();
            return View("Manage", editUser);
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var username = User.Identity.Name;
            var user = await userManager.FindByNameAsync(username);

            if (user == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerID == user.CustomerID);

            if (customer == null)
            {
                return NotFound();
            }

            var emailVM = new ChangeEmailViewModel();
            var passwordVM = new ChangePasswordViewModel();

            var editUser = new UserEditViewModel
            {
                PasswordViewModel = passwordVM,
                EmailViewModel = emailVM,
                User = user,
                Customer = customer
            };

            return View(editUser);
        }

        [HttpPost]
        public async Task<IActionResult> Manage(UserEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.User.UserName);
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerID == user.CustomerID);

                if (user == null || customer == null)
                {
                    return NotFound();
                }

                // Update the user data
                var result = await userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }

                // Update the customer data
                customer.BusinessName = model.Customer.BusinessName;
                customer.FirstName = model.Customer.FirstName;
                customer.LastName = model.Customer.LastName;
                customer.Address = model.Customer.Address;
                customer.City = model.Customer.City;
                customer.State = model.Customer.State;
                customer.Zip = model.Customer.Zip;
                customer.Phone = model.Customer.Phone;

                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();

                TempData["userMessage"] = "User information Successfully Updated";
                return RedirectToAction("Manage");
            }

            return View(model);
        }
    }
}
