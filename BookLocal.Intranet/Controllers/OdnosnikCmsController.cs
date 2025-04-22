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
    public class OdnosnikCmsController : Controller
    {
        private readonly BookLocalContext _context;

        public OdnosnikCmsController(BookLocalContext context)
        {
            _context = context;
        }

        // GET: OdnosnikCms
        public async Task<IActionResult> Index()
        {
            return View(await _context.OdnosnikCms.ToListAsync());
        }

        // GET: OdnosnikCms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var odnosnikCms = await _context.OdnosnikCms
                .FirstOrDefaultAsync(m => m.IdOdnosnika == id);
            if (odnosnikCms == null)
            {
                return NotFound();
            }

            return View(odnosnikCms);
        }

        // GET: OdnosnikCms/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: OdnosnikCms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdOdnosnika,Nazwa,Odnosnik")] OdnosnikCms odnosnikCms)
        {
            if (ModelState.IsValid)
            {
                _context.Add(odnosnikCms);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(odnosnikCms);
        }

        // GET: OdnosnikCms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var odnosnikCms = await _context.OdnosnikCms.FindAsync(id);
            if (odnosnikCms == null)
            {
                return NotFound();
            }
            return View(odnosnikCms);
        }

        // POST: OdnosnikCms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdOdnosnika,Nazwa,Odnosnik")] OdnosnikCms odnosnikCms)
        {
            if (id != odnosnikCms.IdOdnosnika)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(odnosnikCms);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OdnosnikCmsExists(odnosnikCms.IdOdnosnika))
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
            return View(odnosnikCms);
        }

        // GET: OdnosnikCms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var odnosnikCms = await _context.OdnosnikCms
                .FirstOrDefaultAsync(m => m.IdOdnosnika == id);
            if (odnosnikCms == null)
            {
                return NotFound();
            }

            return View(odnosnikCms);
        }

        // POST: OdnosnikCms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var odnosnikCms = await _context.OdnosnikCms.FindAsync(id);
            if (odnosnikCms != null)
            {
                _context.OdnosnikCms.Remove(odnosnikCms);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OdnosnikCmsExists(int id)
        {
            return _context.OdnosnikCms.Any(e => e.IdOdnosnika == id);
        }
    }
}
