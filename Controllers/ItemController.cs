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
    }
}
