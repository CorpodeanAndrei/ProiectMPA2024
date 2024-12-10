using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProiectMPA.Data;
using ProiectMPA.Models;

namespace ProiectMPA.Controllers
{
    [Authorize(Roles = "Employee")]
    public class ChasisTypesController : Controller
    {
        private readonly ProiectMPAContext _context;

        public ChasisTypesController(ProiectMPAContext context)
        {
            _context = context;
        }

        // GET: ChasisTypes
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View(await _context.ChasisType.ToListAsync());
        }

        // GET: ChasisTypes/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chasisType = await _context.ChasisType
                .FirstOrDefaultAsync(m => m.ID == id);
            if (chasisType == null)
            {
                return NotFound();
            }

            return View(chasisType);
        }

        // GET: ChasisTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ChasisTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name")] ChasisType chasisType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chasisType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(chasisType);
        }

        // GET: ChasisTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chasisType = await _context.ChasisType.FindAsync(id);
            if (chasisType == null)
            {
                return NotFound();
            }
            return View(chasisType);
        }

        // POST: ChasisTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name")] ChasisType chasisType)
        {
            if (id != chasisType.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chasisType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChasisTypeExists(chasisType.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(chasisType);
        }

        // GET: ChasisTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chasisType = await _context.ChasisType
                .FirstOrDefaultAsync(m => m.ID == id);
            if (chasisType == null)
            {
                return NotFound();
            }

            return View(chasisType);
        }

        // POST: ChasisTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chasisType = await _context.ChasisType.FindAsync(id);
            if (chasisType != null)
            {
                _context.ChasisType.Remove(chasisType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChasisTypeExists(int id)
        {
            return _context.ChasisType.Any(e => e.ID == id);
        }
    }
}
