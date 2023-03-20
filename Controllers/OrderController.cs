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

        [HttpGet]
        public IActionResult AddOrder()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrder(Order newOrder)
        {
            if (ModelState.IsValid)
            {
                newOrder.OrderCost = 0;
                newOrder.ItemCount = 0;
                newOrder.Completed = false;
                newOrder.Cancelled = false;
                _context.Add(newOrder);
                await _context.SaveChangesAsync();

                ViewBag.AllItems = _context.Items.ToList();
                return RedirectToAction(nameof(ViewItems));
            }

            return View(newOrder);
        }

        [HttpGet]
        public IActionResult ViewItems()
        {
            var allItems = _context.Items.ToList();

            Order order = _context.Orders.OrderByDescending(p => p.OrderID).FirstOrDefault();

            var orderItems = _context.OrderItems.ToList();
            var selectedItems = _context.OrderItems.Include(c => c.Item).ToList();
            selectedItems.Clear();

            foreach (OrderItem selectedItem in orderItems)
            {
                if (selectedItem.OrderID == order.OrderID)
                {
                    selectedItems.Add(selectedItem);
                }
            }
            ViewBag.items = selectedItems;

            return View(allItems);
        }

        [HttpGet]
        public async Task<IActionResult> AddItemToOrder(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            OrderItem orderItem = new OrderItem();

            orderItem.ItemID = item.ItemID;

            Order order = _context.Orders.OrderByDescending(p => p.OrderID).FirstOrDefault();

            //Update order cost and item count
            orderItem.OrderID = order.OrderID;
            order.OrderCost += Convert.ToDecimal(item.Price);
            order.ItemCount += 1;
            _context.Update(order);

            _context.Add(orderItem);
            await _context.SaveChangesAsync();

            
            return RedirectToAction(nameof(ViewItems));
        }

        public async Task<IActionResult> ViewCustOrders()
        {
            //Remove empty orders
            var allOrders = _context.Orders.ToList();
            foreach (Order order in allOrders)
            {
                if(order.OrderCost == 0 && order.ItemCount == 0 && order.Completed == false && order.Cancelled == false)
                {
                    _context.Remove(order);
                    await _context.SaveChangesAsync();
                }
            }

            allOrders = _context.Orders.ToList();
            var custOrders = _context.Orders.ToList();
            custOrders.Clear();

            foreach(Order order in allOrders)
            {
                if (order.CustomerID == order.CustomerID) //User.CustomerID
                {
                    

                    var allOrderItems = _context.OrderItems.ToList();
                    var orderItems = _context.OrderItems.Include(c => c.Item).ToList();
                    orderItems.Clear();

                    foreach (OrderItem item in allOrderItems)
                    {
                        if (item.OrderID == order.OrderID)
                        {
                            orderItems.Add(item);
                            //order.OrderCost += Convert.ToDecimal(item.Item.Price);
                        }
                    }

                    //order.ItemCount = orderItems.Count();
                    custOrders.Add(order);
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
                    //order.OrderCost += Convert.ToDecimal(item.Item.Price);
                }
            }
            
            ViewBag.items = orderItems;
            //order.ItemCount = orderItems.Count();
            return View(order);
        }

        [HttpGet]
        public IActionResult CancelOrder(int? id)
        {
            if (id == null)
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
                if (item.OrderID == order.OrderID)
                {
                    orderItems.Add(item);
                    //order.OrderCost += Convert.ToDecimal(item.Item.Price);
                }
            }

            ViewBag.items = orderItems;
            //order.ItemCount = orderItems.Count();
            return View(order);
        }

        [HttpPost, ActionName("CancelOrder")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCancel(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            order.Cancelled = true;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ViewCustOrders));
        }

        [HttpGet]
        public async Task<IActionResult> RemoveOrder(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = _context.Orders.FirstOrDefault(m => m.OrderID == id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Remove(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ViewCustOrders));
        }

        public IActionResult Search(string searchString)
        {
            if (!String.IsNullOrEmpty(searchString))
            {
                var items = _context.Items.Where(i => i.ItemName!.Contains(searchString));

                Order order = _context.Orders.OrderByDescending(p => p.OrderID).FirstOrDefault();

                var orderItems = _context.OrderItems.ToList();
                var selectedItems = _context.OrderItems.Include(c => c.Item).ToList();
                selectedItems.Clear();

                foreach (OrderItem selectedItem in orderItems)
                {
                    if (selectedItem.OrderID == order.OrderID)
                    {
                        selectedItems.Add(selectedItem);
                    }
                }
                ViewBag.items = selectedItems;

                return View("ViewItems", items);
            }
            return RedirectToAction(nameof(AddItemToOrder));
        }

        public async Task<IActionResult> CancelNewOrder()
        {
            Order order = _context.Orders.OrderByDescending(p => p.OrderID).FirstOrDefault();

            order.Cancelled = true;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ViewCustOrders));
        }

        public async Task<IActionResult> RemoveItemFromOrder(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItem = _context.OrderItems.Include(c => c.Item).FirstOrDefault(m => m.ID == id);
            if (orderItem == null)
            {
                return NotFound();
            }

            // Remove order item
            _context.Remove(orderItem);
            await _context.SaveChangesAsync();

            Order order = _context.Orders.OrderByDescending(p => p.OrderID).FirstOrDefault();

            // Update order cost and item count
            order.OrderCost -= Convert.ToDecimal(orderItem.Item.Price);
            order.ItemCount -= 1;
            _context.Update(order);
            await _context.SaveChangesAsync();

            // Get order items for order
            var orderItems = _context.OrderItems.ToList();
            var selectedItems = _context.OrderItems.Include(c => c.Item).ToList();
            selectedItems.Clear();

            foreach (OrderItem selectedItem in orderItems)
            {
                if (selectedItem.OrderID == order.OrderID)
                {
                    selectedItems.Add(selectedItem);
                }
            }
            ViewBag.items = selectedItems;

            return RedirectToAction(nameof(ViewItems));
        }
    }
}
