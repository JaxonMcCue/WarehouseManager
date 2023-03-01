using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarehouseManager.Data;
using WarehouseManager.Models;

namespace WarehouseManager.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult ViewCustOrders()
        {
            var allOrders = _context.Orders.ToList();
            var custOrders = _context.Orders.ToList();
            custOrders.Clear();

            foreach(Order order in allOrders)
            {
                if (order.CustomerID == order.CustomerID) //User.CustomerID
                {
                    custOrders.Add(order);

                    var allOrderItems = _context.OrderItems.ToList();
                    var orderItems = _context.OrderItems.ToList();
                    orderItems.Clear();

                    foreach (OrderItem item in allOrderItems)
                    {
                        if (item.OrderID == order.OrderID)
                        {
                            orderItems.Add(item);
                        }
                    }

                    order.ItemCount = orderItems.Count();
                }
            }

            return View(custOrders);
        }

        public IActionResult ViewOrder(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var order = _context.Orders.FirstOrDefault(m => m.OrderID == id);
            if (order == null)
            {
                return NotFound();
            }

            var allOrderItems = _context.OrderItems.ToList();
            var orderItems = _context.OrderItems.Include(c => c.Item).ToList();
            orderItems.Clear();

            foreach (OrderItem item in allOrderItems)
            {
                if(item.OrderID == order.OrderID)
                {
                    orderItems.Add(item);
                }
            }
            
            ViewBag.items = orderItems;
            return View(order);
        }

        
    }
}
