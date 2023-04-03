using Microsoft.AspNetCore.Authorization;
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

        public async Task<IActionResult> AddOrder(Order newOrder)
        {
            if (ModelState.IsValid)
            {
                User loggedInUser = null;
                var users = _context.Users.ToList();

                foreach (User user in users)
                {
                    if (user.UserName == User.Identity.Name)
                    {
                        loggedInUser = user;
                    }
                }

                newOrder.CustomerID = (int)loggedInUser.CustomerID;
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

        [HttpPost]
        public async Task<IActionResult> AddItemToOrder(OrderItem OrderItem)
        {
            int count = OrderItem.Count;
            var item = await _context.Items.FirstOrDefaultAsync(i => i.ItemID == OrderItem.ItemID);
            if (item == null)
            {
                return NotFound();
            }

            OrderItem orderItem = new OrderItem();

            orderItem.ItemID = item.ItemID;

            Order order = _context.Orders.OrderByDescending(p => p.OrderID).FirstOrDefault();

            var orderlist = _context.OrderItems.Where(o => o.OrderID == order.OrderID).FirstOrDefault(i => i.ItemID == item.ItemID);

            //checks to see if item is in list, adds to count if there is, otherwise makes new instance
            if (orderlist == null)
            {
                if(count > item.ItemAmount)
                {
                    count = item.ItemAmount;
                }
                orderItem.Count = count;
                _context.Add(orderItem);
            }
            else
            {
                if ((orderlist.Count + count) > item.ItemAmount)
                {
                    count = item.ItemAmount - orderlist.Count;
                }
                orderlist.Count += count;
                _context.Update(orderlist);
            }



            //Update order cost and item count
            orderItem.OrderID = order.OrderID;
            order.OrderCost += Convert.ToDecimal(item.Price) * count;
            order.ItemCount += count;

            _context.Update(order);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ViewItems));
        }

        [Authorize]
        public async Task<IActionResult> ViewCustOrders()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(DisplayOrders));
            }
            if (User.IsInRole("Sales"))
            {
                return RedirectToAction(nameof(DisplayIncompleteOrders));
            }

            //Remove empty orders
            var allOrders = _context.Orders.ToList();
            foreach (Order order in allOrders)
            {
                if (order.OrderCost == 0 && order.ItemCount == 0 && order.Completed == false && order.Cancelled == false)
                {
                    _context.Remove(order);
                    await _context.SaveChangesAsync();
                }
            }

            User loggedInUser = null;
            allOrders = _context.Orders.ToList();
            var custOrders = _context.Orders.ToList();
            custOrders.Clear();

            //Get logged in user
            var users = _context.Users.ToList();

            foreach (User user in users)
            {
                if (user.UserName == User.Identity.Name)
                {
                    loggedInUser = user;
                }
            }

            foreach (Order order in allOrders)
            {
                if (order.CustomerID == loggedInUser.CustomerID) //User.CustomerID
                {
                    var allOrderItems = _context.OrderItems.ToList();
                    var orderItems = _context.OrderItems.Include(c => c.Item).ToList();
                    orderItems.Clear();

                    foreach (OrderItem item in allOrderItems)
                    {
                        if (item.OrderID == order.OrderID)
                        {
                            orderItems.Add(item);
                        }
                    }

                    custOrders.Add(order);
                }
            }

            return View(custOrders);
        }

        public IActionResult ViewOrder(int? id)
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

            //Check if customerid matches with orders customerid
            User loggedInUser = null;
            var users = _context.Users.ToList();

            foreach (User user in users)
            {
                if (user.UserName == User.Identity.Name)
                {
                    loggedInUser = user;
                }
            }

            if (!User.IsInRole("Admin") && !User.IsInRole("Sales"))
            {
                if (loggedInUser.CustomerID != order.CustomerID)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }
            }

            var allOrderItems = _context.OrderItems.ToList();
            var orderItems = _context.OrderItems.Include(c => c.Item).ToList();
            orderItems.Clear();

            foreach (OrderItem item in allOrderItems)
            {
                if (item.OrderID == order.OrderID)
                {
                    orderItems.Add(item);
                }
            }

            //Get customer
            var customers = _context.Customers.ToList();
            Customer orderCustomer = null;

            foreach (Customer customer in customers)
            {
                if (customer.CustomerID == order.CustomerID)
                {
                    orderCustomer = customer;
                }
            }

            ViewBag.customer = orderCustomer;
            ViewBag.items = orderItems;
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

            //Check if customerid matches with orders customerid
            User loggedInUser = null;
            var users = _context.Users.ToList();

            foreach (User user in users)
            {
                if (user.UserName == User.Identity.Name)
                {
                    loggedInUser = user;
                }
            }

            if (!User.IsInRole("Admin"))
            {
                if (loggedInUser.CustomerID != order.CustomerID)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }
            }

            var allOrderItems = _context.OrderItems.ToList();
            var orderItems = _context.OrderItems.Include(c => c.Item).ToList();
            orderItems.Clear();

            foreach (OrderItem item in allOrderItems)
            {
                if (item.OrderID == order.OrderID)
                {
                    orderItems.Add(item);
                }
            }

            //Get customer
            var customers = _context.Customers.ToList();
            Customer orderCustomer = null;

            foreach (Customer customer in customers)
            {
                if (customer.CustomerID == order.CustomerID)
                {
                    orderCustomer = customer;
                }
            }

            ViewBag.customer = orderCustomer;
            ViewBag.items = orderItems;
            return View(order);
        }

        [HttpPost, ActionName("CancelOrder")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCancel(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            order.Cancelled = true;
            await _context.SaveChangesAsync();
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(DisplayOrders));
            }

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

            _context.Remove(order);
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
            order.OrderCost -= Convert.ToDecimal(orderItem.Item.Price) * orderItem.Count;
            order.ItemCount -= orderItem.Count;
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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DisplayOrders()
        {
            //Remove empty orders
            var allOrders = _context.Orders.OrderBy(p => p.OrderID).ToList();
            foreach (Order order in allOrders)
            {
                if (order.OrderCost == 0 && order.ItemCount == 0 && order.Completed == false && order.Cancelled == false)
                {
                    _context.Remove(order);
                    await _context.SaveChangesAsync();
                }
            }

            return View(allOrders);
        }

        [Authorize(Roles = "Sales,Admin")]
        public async Task<IActionResult> DisplayIncompleteOrders()
        {
            //Remove empty orders
            var allOrders = _context.Orders.ToList();
            foreach (Order order in allOrders)
            {
                if (order.OrderCost == 0 && order.ItemCount == 0 && order.Completed == false && order.Cancelled == false)
                {
                    _context.Remove(order);
                    await _context.SaveChangesAsync();
                }
            }

            allOrders = _context.Orders.ToList();
            var incompleteOrders = _context.Orders.ToList();
            incompleteOrders.Clear();

            foreach (Order order in allOrders)
            {
                if (order.Completed == false && order.Cancelled == false)
                {
                    incompleteOrders.Add(order);
                }
            }

            incompleteOrders.OrderBy(p => p.OrderID);

            return View(incompleteOrders);
        }

        [HttpGet]
        [Authorize(Roles = "Sales,Admin")]
        public async Task<IActionResult> CompleteOrder(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FirstOrDefaultAsync(m => m.OrderID == id);
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
                }
            }

            var customers = _context.Customers.ToList();
            Customer orderCustomer = null;

            foreach (Customer customer in customers)
            {
                if (customer.CustomerID == order.CustomerID)
                {
                    orderCustomer = customer;
                }
            }

            ViewBag.customer = orderCustomer;
            ViewBag.items = orderItems;
            return View(order);
        }

        [HttpPost, ActionName("CompleteOrder")]
        [Authorize(Roles = "Sales,Admin")]
        public async Task<IActionResult> ConfirmComplete(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            order.Completed = true;
            var orderItems = _context.OrderItems.Where(o => o.OrderID == order.OrderID);
            foreach (OrderItem oItem in orderItems)
            {
                var item = await _context.Items.FirstOrDefaultAsync(i => i.ItemID == oItem.ItemID);
                item.ItemAmount -= 1;
                _context.Update(item);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(DisplayIncompleteOrders));
        }

        [HttpGet]
        public async Task<IActionResult> Reorder(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FirstOrDefaultAsync(m => m.OrderID == id);
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
                    OrderItem newOrderItem = new OrderItem();
                    newOrderItem.ItemID = item.ItemID;
                    orderItems.Add(newOrderItem);
                }
            }

            Order newOrder = new Order();

            newOrder.CustomerID = order.CustomerID;
            newOrder.OrderCost = 0;
            newOrder.ItemCount = 0;
            newOrder.Completed = false;
            newOrder.Cancelled = false;

            _context.Add(newOrder);
            await _context.SaveChangesAsync();

            newOrder = _context.Orders.OrderByDescending(p => p.OrderID).FirstOrDefault();

            var newOrderItems = _context.OrderItems.Include(c => c.Item).ToList();
            newOrderItems.Clear();

            foreach (OrderItem item in orderItems)
            {
                item.OrderID = newOrder.OrderID;
                newOrderItems.Add(item);
            }

            foreach (OrderItem item in newOrderItems)
            {
                _context.Add(item);
            }
            await _context.SaveChangesAsync();

            newOrder.OrderCost = order.OrderCost;
            newOrder.ItemCount = order.ItemCount;

            Customer customer = await _context.Customers.FirstOrDefaultAsync(m => m.CustomerID == newOrder.CustomerID);

            ViewBag.customer = customer;
            ViewBag.orderItems = newOrderItems;

            return View(newOrder);
        }

        [HttpGet]
        public async Task<IActionResult> ReorderConfirm(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FirstOrDefaultAsync(m => m.OrderID == id);
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
                    order.ItemCount += 1;
                    order.OrderCost += Convert.ToDecimal(item.Item.Price);
                    orderItems.Add(item);
                }
            }

            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ViewCustOrders));
        }
    }
}
