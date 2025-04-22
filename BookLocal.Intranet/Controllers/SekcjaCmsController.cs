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
    public class SekcjaCmsController : Controller
    {
        private readonly BookLocalContext _context;

        public SekcjaCmsController(BookLocalContext context)
        {
            _context = context;
        }

        // GET: SekcjaCms
        public async Task<IActionResult> Index()
        {
            var bookLocalContext = _context.SekcjaCms.Include(s => s.LastModifiedByPracownik);
            return View(await bookLocalContext.ToListAsync());
        }

        // GET: SekcjaCms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sekcjaCms = await _context.SekcjaCms
                .Include(s => s.LastModifiedByPracownik)
                .FirstOrDefaultAsync(m => m.IdSekcji == id);
            if (sekcjaCms == null)
            {
                return NotFound();
            }

            return View(sekcjaCms);
        }

        // GET: SekcjaCms/Create
        public IActionResult Create()
        {
            ViewData["LastModifiedByPracownikId"] = new SelectList(_context.Pracownik, "IdPracownika", "Imie");
            return View();
        }

        // POST: SekcjaCms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdSekcji,KluczSekcji,Kolejnosc,LastModifiedByPracownikId,LastModifiedDate")] SekcjaCms sekcjaCms)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sekcjaCms);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LastModifiedByPracownikId"] = new SelectList(_context.Pracownik, "IdPracownika", "Imie", sekcjaCms.LastModifiedByPracownikId);
            return View(sekcjaCms);
        }

        // GET: SekcjaCms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sekcjaCms = await _context.SekcjaCms.FindAsync(id);
            if (sekcjaCms == null)
            {
                return NotFound();
            }
            ViewData["LastModifiedByPracownikId"] = new SelectList(_context.Pracownik, "IdPracownika", "Imie", sekcjaCms.LastModifiedByPracownikId);
            return View(sekcjaCms);
        }

        // POST: SekcjaCms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdSekcji,KluczSekcji,Kolejnosc,LastModifiedByPracownikId,LastModifiedDate")] SekcjaCms sekcjaCms)
        {
            if (id != sekcjaCms.IdSekcji)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sekcjaCms);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SekcjaCmsExists(sekcjaCms.IdSekcji))
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
            ViewData["LastModifiedByPracownikId"] = new SelectList(_context.Pracownik, "IdPracownika", "Imie", sekcjaCms.LastModifiedByPracownikId);
            return View(sekcjaCms);
        }

        // GET: SekcjaCms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sekcjaCms = await _context.SekcjaCms
                .Include(s => s.LastModifiedByPracownik)
                .FirstOrDefaultAsync(m => m.IdSekcji == id);
            if (sekcjaCms == null)
            {
                return NotFound();
            }

            return View(sekcjaCms);
        }

        // POST: SekcjaCms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sekcjaCms = await _context.SekcjaCms.FindAsync(id);
            if (sekcjaCms != null)
            {
                _context.SekcjaCms.Remove(sekcjaCms);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SekcjaCmsExists(int id)
        {
            return _context.SekcjaCms.Any(e => e.IdSekcji == id);
        }
    }
}
