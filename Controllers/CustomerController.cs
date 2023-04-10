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
    [Authorize(Roles = "Admin,Sales")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var customers = _context.Customers.Join(_context.Users, c => c.CustomerID, u => u.CustomerID,
                (c, u) => new CustomerViewModel
                {
                    CustomerID = c.CustomerID,
                    BusinessName = c.BusinessName,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Address = c.Address,
                    City = c.City,
                    State = c.State,
                    Zip = c.Zip,
                    Phone = c.Phone,
                    Email = u.Email
                }).ToList();

            return View(customers);

        }

        public IActionResult Search(string searchString)
        {
            var customers = _context.Customers
            .Join(_context.Users, c => c.CustomerID, u => u.CustomerID,
            (c, u) => new CustomerViewModel
            {
                CustomerID = c.CustomerID,
                BusinessName = c.BusinessName,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Address = c.Address,
                City = c.City,
                State = c.State,
                Zip = c.Zip,
                Phone = c.Phone,
                Email = u.Email
            });

            if (!String.IsNullOrEmpty(searchString))
            {
                customers = customers.Where(c => c.BusinessName.Contains(searchString) ||
                                            c.FirstName.Contains(searchString) ||
                                            c.LastName.Contains(searchString) ||
                                            c.City.Equals(searchString) ||
                                            c.State.Equals(searchString));
            }
            return View("Index", customers.ToList());
        }
    }
}
