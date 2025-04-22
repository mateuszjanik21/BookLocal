using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookLocal.Data.Data;
using BookLocal.Data.Data.CMS;

namespace BookLocal.Intranet.Controllers
{
    public class NaglowekCmsController : Controller
    {
        private readonly BookLocalContext _context;

        public NaglowekCmsController(BookLocalContext context)
        {
            _context = context;
        }

        // GET: NaglowekCms
        public async Task<IActionResult> Index()
        {
            return View(await _context.NaglowekCms.ToListAsync());
        }

        // GET: NaglowekCms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var naglowekCms = await _context.NaglowekCms
                .FirstOrDefaultAsync(m => m.IdNapisu == id);
            if (naglowekCms == null)
            {
                return NotFound();
            }

            return View(naglowekCms);
        }

        // GET: NaglowekCms/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: NaglowekCms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdNapisu,Naglowek,Tresc")] NaglowekCms naglowekCms)
        {
            if (ModelState.IsValid)
            {
                _context.Add(naglowekCms);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(naglowekCms);
        }

        // GET: NaglowekCms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var naglowekCms = await _context.NaglowekCms.FindAsync(id);
            if (naglowekCms == null)
            {
                return NotFound();
            }
            return View(naglowekCms);
        }

        // POST: NaglowekCms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdNapisu,Naglowek,Tresc")] NaglowekCms naglowekCms)
        {
            if (id != naglowekCms.IdNapisu)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(naglowekCms);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NaglowekCmsExists(naglowekCms.IdNapisu))
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
            return View(naglowekCms);
        }

        // GET: NaglowekCms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var naglowekCms = await _context.NaglowekCms
                .FirstOrDefaultAsync(m => m.IdNapisu == id);
            if (naglowekCms == null)
            {
                return NotFound();
            }

            return View(naglowekCms);
        }

        // POST: NaglowekCms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var naglowekCms = await _context.NaglowekCms.FindAsync(id);
            if (naglowekCms != null)
            {
                _context.NaglowekCms.Remove(naglowekCms);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NaglowekCmsExists(int id)
        {
            return _context.NaglowekCms.Any(e => e.IdNapisu == id);
        }
    }
}
