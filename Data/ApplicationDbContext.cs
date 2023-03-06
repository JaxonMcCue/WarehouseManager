using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WarehouseManager.Models;

namespace WarehouseManager.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

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
