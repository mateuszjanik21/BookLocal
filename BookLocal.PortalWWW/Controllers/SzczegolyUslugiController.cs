using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookLocal.Data.Data;
using BookLocal.Data.Data.PlatformaInternetowa;

namespace BookLocal.PortalWWW.Controllers
{
    public class SzczegolyUslugiController : Controller
    {
        private readonly BookLocalContext _context;

        public SzczegolyUslugiController(BookLocalContext context)
        {
            _context = context;
        }

        // GET: SzczegolyUslugi
        public async Task<IActionResult> Index()
        {
            ViewBag.ModelUslugi = await _context.Usluga
                                             .ToListAsync();

            var bookLocalContext = _context.SzczegolyUslugi.Include(s => s.Pracownik).Include(s => s.Usluga);
            return View(await bookLocalContext.ToListAsync());
        }

        // GET: SzczegolyUslugi/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var szczegolyUslugi = await _context.SzczegolyUslugi
                .Include(s => s.Pracownik)
                .Include(s => s.Usluga)
                .FirstOrDefaultAsync(m => m.IdSzczegolowUslugi == id);
            if (szczegolyUslugi == null)
            {
                return NotFound();
            }

            return View(szczegolyUslugi);
        }

        // GET: SzczegolyUslugi/Create
        public IActionResult Create()
        {
            ViewData["PracownikId"] = new SelectList(_context.Pracownik, "IdPracownika", "Imie");
            ViewData["UslugaId"] = new SelectList(_context.Usluga, "IdUslugi", "Nazwa");
            return View();
        }

        // POST: SzczegolyUslugi/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdSzczegolowUslugi,Opis,Cena,CzasTrwaniaMinuty,UslugaId,PracownikId")] SzczegolyUslugi szczegolyUslugi)
        {
            if (ModelState.IsValid)
            {
                _context.Add(szczegolyUslugi);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PracownikId"] = new SelectList(_context.Pracownik, "IdPracownika", "Imie", szczegolyUslugi.PracownikId);
            ViewData["UslugaId"] = new SelectList(_context.Usluga, "IdUslugi", "Nazwa", szczegolyUslugi.UslugaId);
            return View(szczegolyUslugi);
        }

        // GET: SzczegolyUslugi/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var szczegolyUslugi = await _context.SzczegolyUslugi.FindAsync(id);
            if (szczegolyUslugi == null)
            {
                return NotFound();
            }
            ViewData["PracownikId"] = new SelectList(_context.Pracownik, "IdPracownika", "Imie", szczegolyUslugi.PracownikId);
            ViewData["UslugaId"] = new SelectList(_context.Usluga, "IdUslugi", "Nazwa", szczegolyUslugi.UslugaId);
            return View(szczegolyUslugi);
        }

        // POST: SzczegolyUslugi/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdSzczegolowUslugi,Opis,Cena,CzasTrwaniaMinuty,UslugaId,PracownikId")] SzczegolyUslugi szczegolyUslugi)
        {
            if (id != szczegolyUslugi.IdSzczegolowUslugi)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(szczegolyUslugi);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SzczegolyUslugiExists(szczegolyUslugi.IdSzczegolowUslugi))
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
            ViewData["PracownikId"] = new SelectList(_context.Pracownik, "IdPracownika", "Imie", szczegolyUslugi.PracownikId);
            ViewData["UslugaId"] = new SelectList(_context.Usluga, "IdUslugi", "Nazwa", szczegolyUslugi.UslugaId);
            return View(szczegolyUslugi);
        }

        // GET: SzczegolyUslugi/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var szczegolyUslugi = await _context.SzczegolyUslugi
                .Include(s => s.Pracownik)
                .Include(s => s.Usluga)
                .FirstOrDefaultAsync(m => m.IdSzczegolowUslugi == id);
            if (szczegolyUslugi == null)
            {
                return NotFound();
            }

            return View(szczegolyUslugi);
        }

        // POST: SzczegolyUslugi/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var szczegolyUslugi = await _context.SzczegolyUslugi.FindAsync(id);
            if (szczegolyUslugi != null)
            {
                _context.SzczegolyUslugi.Remove(szczegolyUslugi);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SzczegolyUslugiExists(int id)
        {
            return _context.SzczegolyUslugi.Any(e => e.IdSzczegolowUslugi == id);
        }
    }
}
