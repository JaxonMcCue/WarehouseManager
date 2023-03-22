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
    public class ItemController : Controller
    {

        private readonly ApplicationDbContext _context;

        public ItemController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult DisplayItems()
        {
            return View(_context.Items.ToList());
        }

        public IActionResult LowItems()
        {
            var items = _context.Items.Where(i => i.ItemAmount < 5);
            return View("DisplayItems", items);
        }

        [HttpGet]
        public IActionResult AddItem()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(Item item)
        {
            if (ModelState.IsValid)
            {
                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(DisplayItems));
            }
            return View(item);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteItem(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var item = await _context.Items.FirstOrDefaultAsync(i => i.ItemID == id);
            if(item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost, ActionName("DeleteItem")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItemConfirmed(int id)
        {
            var item = await _context.Items.FindAsync(id);
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(DisplayItems));
        }

        public async Task<IActionResult> ViewItem(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var item = await _context.Items.FirstOrDefaultAsync(i => i.ItemID == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        public async Task<IActionResult> EditItem(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items.FirstOrDefaultAsync(i => i.ItemID == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditItem(int id, Item item)
        {
            if (id != item.ItemID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!(_context.Items.Any(i => i.ItemID == item.ItemID)))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(DisplayItems));
            }
            return View(item);
        }

        public IActionResult EditAmt()
        {
            return View(_context.Items.ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Item/ItemAmount")]
        public async Task<IActionResult> EditAmount(Item newItem)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i => i.ItemID == newItem.ItemID);
            if (item == null)
            {
                return NotFound();
            }

            if(newItem.ItemAmount != item.ItemAmount)
            {
                item.ItemAmount = newItem.ItemAmount;
                try
                {
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!(_context.Items.Any(i => i.ItemID == item.ItemID)))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return RedirectToAction(nameof(EditAmt));
        }

        public async Task<IActionResult> Search(string searchString)
        {
            if (!String.IsNullOrEmpty(searchString))
            {
                var items = _context.Items.Where(i => i.ItemName!.Contains(searchString));
                return View("DisplayItems", items);
            }
            return RedirectToAction(nameof(DisplayItems));
        }

        public async Task<IActionResult> SearchAmt(string searchString)
        {
            if (!String.IsNullOrEmpty(searchString))
            {
                var items = _context.Items.Where(i => i.ItemName!.Contains(searchString));
                return View("EditAmt", items);
            }
            return RedirectToAction(nameof(EditAmt));
        }

        public async Task<IActionResult> ReportItem(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items.FirstOrDefaultAsync(i => i.ItemID == id);
            if (item == null)
            {
                return NotFound();
            }

            var damage = new DamagedItem();

            damage.Count = 0;
            damage.ItemID = item.ItemID;

            return View(damage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportItem(DamagedItem damagedItem)
        {
            damagedItem.Date = DateTime.Now;
            if (ModelState.IsValid)
            {
                var item = await _context.Items.FirstOrDefaultAsync(i => i.ItemID == damagedItem.ItemID);
                item.ItemAmount -= damagedItem.Count;

                _context.Damaged.Add(damagedItem);
                _context.Items.Update(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(EditAmt));
            }
            return View(damagedItem);
        }
    }
}
