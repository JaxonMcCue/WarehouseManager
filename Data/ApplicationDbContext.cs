
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WarehouseManager.Models;

namespace WarehouseManager.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public static async Task CreateAdminUser(IServiceProvider serviceProvider)
        {
            UserManager<User> userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            RoleManager<Role> roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();

            string userName = "admin";
            string password = "placeHolder";
            List<string> roleNames = new List<string> { "Admin", "Sales" };


            foreach (var role in roleNames)
            {
                if (await roleManager.FindByNameAsync(role) == null)
                {
                    await roleManager.CreateAsync(new Role(role));
                }
            }


            if (await userManager.FindByNameAsync(userName) == null)
            {
                User user = new User { UserName = userName };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<DamagedItem> Damaged { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Order>().HasData(
                new Order
                {
                    OrderID = 1,
                    CustomerID = 1,
                    OrderCost = 0,
                    Completed = false,
                    Cancelled = false,
                }
                );
            modelBuilder.Entity<Item>().HasData(
                new Item
                {
                    ItemID = 1,
                    ItemName = "Test Item",
                    ItemDescription = "This is a test item",
                    Price = 1.5,
                    ItemAmount = 1,
                }
                );
            modelBuilder.Entity<OrderItem>().HasData(
                new OrderItem
                {
                    ID = 1,
                    ItemID = 1,
                    OrderID = 1,
                }
                );
        }
    }
}
