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

        //[HttpGet]
        //public IActionResult AddOrder()
        //{
        //    var allItems = _context.Items.ToList();
        //    List<ItemViewModel> itemViewList = new List<ItemViewModel>();

        //    foreach (Item item in allItems)
        //    {
        //        ItemViewModel itemView = new ItemViewModel();
        //        itemView.ItemID = item.ItemID;
        //        itemView.ItemName = item.ItemName;
        //        itemView.ItemDescription = item.ItemDescription;
        //        itemView.Price = item.Price;
        //        itemView.ItemAmount = item.ItemAmount;
        //        itemView.ItemSelected = false;
        //        itemViewList.Add(itemView);
        //    }

        //    ViewBag.allItems = itemViewList;
        //    return View();
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AddOrder(Order newOrder)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        newOrder.Completed = false;
        //        newOrder.Cancelled = false;

        //        List<ItemViewModel> selectedItems = new List<ItemViewModel>();

        //        foreach (ItemViewModel item in ViewBag.allItems)
        //        {
        //            if (item.ItemSelected == true) {
        //                OrderItem orderItem = new OrderItem();
        //                orderItem.OrderID = newOrder.OrderID;
        //                orderItem.ItemID = item.ItemID;
        //                _context.Add(orderItem);

        //                newOrder.OrderCost += Convert.ToDecimal(item.Price);
        //                newOrder.ItemCount += 1;

        //                selectedItems.Add(item);
        //            }
        //        }

        //        if(selectedItems.Count > 0)
        //        {
        //            _context.Add(newOrder);
        //            await _context.SaveChangesAsync();
        //        }

        //        return RedirectToAction(nameof(ViewCustOrders));
        //    }

        //    return View(newOrder);
        //}

        public IActionResult ViewCustOrders()
        {
            var allOrders = _context.Orders.ToList();
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
                            order.OrderCost += Convert.ToDecimal(item.Item.Price);
                        }
                    }

                    order.ItemCount = orderItems.Count();
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
                    order.OrderCost += Convert.ToDecimal(item.Item.Price);
                }
            }
            
            ViewBag.items = orderItems;
            order.ItemCount = orderItems.Count();
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
                    order.OrderCost += Convert.ToDecimal(item.Item.Price);
                }
            }

            ViewBag.items = orderItems;
            order.ItemCount = orderItems.Count();
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
    }
}
