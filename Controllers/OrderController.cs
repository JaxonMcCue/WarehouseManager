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

            var selectedItems = _context.OrderItems.Include(c => c.Item).Where(o => o.OrderID == order.OrderID);

            ViewBag.items = selectedItems;

            return View(allItems);
        }

        [HttpPost]
        public async Task<IActionResult> AddItemToOrder(OrderItem OrderItem)
        {
            int count = OrderItem.Count;
            if(count <= 0)
            {
                return RedirectToAction(nameof(ViewItems));
            }

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
                if (count > item.ItemAmount)
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

            orderItem.OrderID = order.OrderID;
            await _context.SaveChangesAsync();

            //Update order cost and item count
            order.OrderCost += Convert.ToDecimal(item.Price) * count;
            order.ItemCount += count;

            _context.Update(order);

            //await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ViewItems));
        }

        public async Task<IActionResult> AddOrderConfirm()
        {
            Order order = await _context.Orders.OrderByDescending(p => p.OrderID).FirstOrDefaultAsync();

            var orderItems = _context.OrderItems.Include(p => p.Item).Where(i => i.OrderID == order.OrderID);

            foreach (OrderItem item in orderItems)
            {
                order.ItemCount += item.Count;
                order.OrderCost += Convert.ToDecimal(item.Item.Price) * item.Count;
            }

            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ViewCustOrders));
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

                    var orderItems = _context.OrderItems.Where(i => i.OrderID == order.OrderID);

                    foreach (OrderItem item in orderItems)
                    {
                        _context.Remove(item);
                    }
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

            var missingItem = new List<Order>();

            foreach (Order order in allOrders)
            {
                if (order.CustomerID == loggedInUser.CustomerID) //User.CustomerID
                {
                    var orderItems = _context.OrderItems.Include(c => c.Item).Where(o => o.OrderID == order.OrderID);

                    foreach (OrderItem item in orderItems)
                    {
                        var lookitem = _context.Items.FirstOrDefault(i => i.ItemID == item.ItemID);
                        if (lookitem.ItemAmount < item.Count)
                        {
                            if (!missingItem.Contains(order))
                            {
                                missingItem.Add(order);
                            }
                        }
                    }

                    custOrders.Add(order);
                }
            }

            ViewBag.Missing = missingItem;

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

            var orderItems = _context.OrderItems.Include(i => i.Item).Where(i => i.OrderID == order.OrderID);
            var missingItem = new List<OrderItem>();

            foreach (OrderItem item in orderItems)
            {
                var lookItem = _context.Items.FirstOrDefault(i => i.ItemID == item.ItemID);
                if (lookItem.ItemAmount < item.Count)
                {
                    missingItem.Add(item);
                }
            }

            //Get customer
            var orderCustomer = _context.Customers.FirstOrDefault(c => c.CustomerID == order.CustomerID);

            ViewBag.Missing = missingItem;
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

            var orderItems = _context.OrderItems.Include(c => c.Item).Where(o => o.OrderID == order.OrderID);

            //Get customer
            var orderCustomer = _context.Customers.FirstOrDefault(c => c.CustomerID == order.CustomerID);

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

            var orderItems = _context.OrderItems.Where(o => o.OrderID == order.OrderID);

            foreach (OrderItem item in orderItems)
            {
                _context.Remove(item);
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ViewCustOrders));
        }

        public IActionResult Search(string searchString)
        {
            if (!String.IsNullOrEmpty(searchString))
            {
                var items = _context.Items.Where(i => i.ItemName!.Contains(searchString));

                Order order = _context.Orders.OrderByDescending(p => p.OrderID).FirstOrDefault();

                var selectedItems = _context.OrderItems.Include(c => c.Item).Where(o => o.OrderID == order.OrderID);

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

            var orderItems = _context.OrderItems.Where(o => o.OrderID == order.OrderID);

            foreach (OrderItem item in orderItems)
            {
                _context.Remove(item);
            }
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
            //order.OrderCost -= Convert.ToDecimal(orderItem.Item.Price) * orderItem.Count;
            //order.ItemCount -= orderItem.Count;
            //_context.Update(order);
            //await _context.SaveChangesAsync();

            // Get order items for order
            var selectedItems = _context.OrderItems.Include(c => c.Item).Where(o => o.OrderID == order.OrderID);

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

                    var orderItems = _context.OrderItems.Where(o => o.OrderID == order.OrderID);

                    foreach (OrderItem item in orderItems)
                    {
                        _context.Remove(item);
                    }
                    await _context.SaveChangesAsync();
                }
            }

            return View(allOrders);
        }


        [Authorize(Roles = "Sales,Admin")]
        public async Task<IActionResult> DisplayIncompleteOrders()
        {
            //Remove empty orders
            var allOrders = _context.Orders.Where(o => o.OrderCost == 0 && o.OrderCost == 0 && o.Completed == false && o.Cancelled == false).ToList();
            foreach (Order order in allOrders)
            {
                _context.Remove(order);
                await _context.SaveChangesAsync();

                var orderItems = _context.OrderItems.Where(i => i.OrderID == order.OrderID).ToList();

                foreach (OrderItem item in orderItems)
                {
                    _context.Remove(item);
                }
                await _context.SaveChangesAsync();
            }

            var incompleteOrders = _context.Orders.Where(o => o.Completed == false && o.Cancelled == false).ToList();
            var missingItem = new List<Order>();

            foreach (Order order in incompleteOrders)
            {
                var orderitems = _context.OrderItems.Where(o => o.OrderID == order.OrderID);
                foreach (OrderItem item in orderitems)
                {
                    var lookItem = _context.Items.FirstOrDefault(i => i.ItemID == item.ItemID);
                    if (lookItem.ItemAmount < item.Count)
                    {
                        if (!missingItem.Contains(order))
                        {
                            missingItem.Add(order);
                        }
                    }
                }
            }

            incompleteOrders.OrderBy(p => p.OrderID);

            ViewBag.Missing = missingItem;
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

            var orderItems = _context.OrderItems.Include(c => c.Item).Where(o => o.OrderID == order.OrderID).ToList();
            var missingItem = new List<OrderItem>();

            foreach (OrderItem item in orderItems)
            {
                var lookItem = _context.Items.FirstOrDefault(i => i.ItemID == item.ItemID);
                if (lookItem.ItemAmount < item.Count)
                {
                    missingItem.Add(item);
                }
            }

            Customer orderCustomer = _context.Customers.FirstOrDefault(c => c.CustomerID == order.CustomerID);

            ViewBag.Missing = missingItem;
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
                item.ItemAmount -= oItem.Count;
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

            var allOrderItems = _context.OrderItems.Where(i => i.OrderID == order.OrderID).ToList();
            var orderItems = _context.OrderItems.Include(c => c.Item).ToList();
            orderItems.Clear();
            var missingItem = new List<OrderItem>();

            foreach (OrderItem item in allOrderItems)
            {
                OrderItem newOrderItem = new OrderItem();
                newOrderItem.ItemID = item.ItemID;
                newOrderItem.Count = item.Count;
                orderItems.Add(newOrderItem);

                var lookItem = _context.Items.FirstOrDefault(i => i.ItemID == item.ItemID);
                if (lookItem.ItemAmount < item.Count)
                {
                    missingItem.Add(newOrderItem);
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

            ViewBag.Missing = missingItem;
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

            var allOrderItems = _context.OrderItems.Where(i => i.OrderID == order.OrderID).ToList();
            var orderItems = _context.OrderItems.Include(c => c.Item).ToList();
            orderItems.Clear();

            foreach (OrderItem item in allOrderItems)
            {
                order.ItemCount += 1;
                order.OrderCost += Convert.ToDecimal(item.Item.Price);
                orderItems.Add(item);
            }

            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ViewCustOrders));
        }

        [Authorize]
        public IActionResult requestRefund(int Id)
        {
            ViewBag.Order = Id;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> requestRefund(Refund refund, int Id)
        {
            if (refund.Reason != "" && refund.Reason != null)
            {
                refund.OrderID = Id;
                _context.Add(refund);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ViewOrder), new { id = Id });
            }
            ViewBag.Error = "A reason must be entered";
            ViewBag.Order = Id;
            return View(refund);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult ViewRefunds()
        {
            return View(_context.Refund.OrderBy(r => r.Confirmed).ToList());
        }
    }
}
