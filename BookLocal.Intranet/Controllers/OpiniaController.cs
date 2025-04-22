using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookLocal.Data.Data;
using BookLocal.Data.Data.PlatformaInternetowa;

namespace BookLocal.Intranet.Controllers
{
    public class OpiniaController : Controller
    {
        private readonly BookLocalContext _context;

        public OpiniaController(BookLocalContext context)
        {
            _context = context;
        }

        // GET: Opinia
        public async Task<IActionResult> Index()
        {
            var BookLocalContext = _context.Opinia.Include(o => o.OcenianyPracownik).Include(o => o.Rezerwacja).Include(o => o.Uzytkownik);
            return View(await BookLocalContext.ToListAsync());
        }

        // GET: Opinia/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var opinia = await _context.Opinia
                .Include(o => o.OcenianyPracownik)
                .Include(o => o.Rezerwacja)
                .Include(o => o.Uzytkownik)
                .FirstOrDefaultAsync(m => m.IdOpinii == id);
            if (opinia == null)
            {
                return NotFound();
            }

            return View(opinia);
        }

        // GET: Opinia/Create
        public IActionResult Create()
        {
            ViewData["OcenianyPracownikId"] = new SelectList(_context.Set<Pracownik>(), "IdPracownika", "Imie");
            ViewData["RezerwacjaId"] = new SelectList(_context.Set<Rezerwacja>(), "IdRezerwacji", "Status");
            ViewData["UzytkownikId"] = new SelectList(_context.Set<Uzytkownik>(), "IdUzytkownika", "Email");
            return View();
        }

        // POST: Opinia/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdOpinii,Tresc,Ocena,UzytkownikId,OcenianyPracownikId,RezerwacjaId,DataDodania")] Opinia opinia)
        {
            if (ModelState.IsValid)
            {
                _context.Add(opinia);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OcenianyPracownikId"] = new SelectList(_context.Set<Pracownik>(), "IdPracownika", "Imie", opinia.OcenianyPracownikId);
            ViewData["RezerwacjaId"] = new SelectList(_context.Set<Rezerwacja>(), "IdRezerwacji", "Status", opinia.RezerwacjaId);
            ViewData["UzytkownikId"] = new SelectList(_context.Set<Uzytkownik>(), "IdUzytkownika", "Email", opinia.UzytkownikId);
            return View(opinia);
        }

        // GET: Opinia/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var opinia = await _context.Opinia.FindAsync(id);
            if (opinia == null)
            {
                return NotFound();
            }
            ViewData["OcenianyPracownikId"] = new SelectList(_context.Set<Pracownik>(), "IdPracownika", "Imie", opinia.OcenianyPracownikId);
            ViewData["RezerwacjaId"] = new SelectList(_context.Set<Rezerwacja>(), "IdRezerwacji", "Status", opinia.RezerwacjaId);
            ViewData["UzytkownikId"] = new SelectList(_context.Set<Uzytkownik>(), "IdUzytkownika", "Email", opinia.UzytkownikId);
            return View(opinia);
        }

        // POST: Opinia/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdOpinii,Tresc,Ocena,UzytkownikId,OcenianyPracownikId,RezerwacjaId,DataDodania")] Opinia opinia)
        {
            if (id != opinia.IdOpinii)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(opinia);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OpiniaExists(opinia.IdOpinii))
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
            ViewData["OcenianyPracownikId"] = new SelectList(_context.Set<Pracownik>(), "IdPracownika", "Imie", opinia.OcenianyPracownikId);
            ViewData["RezerwacjaId"] = new SelectList(_context.Set<Rezerwacja>(), "IdRezerwacji", "Status", opinia.RezerwacjaId);
            ViewData["UzytkownikId"] = new SelectList(_context.Set<Uzytkownik>(), "IdUzytkownika", "Email", opinia.UzytkownikId);
            return View(opinia);
        }

        // GET: Opinia/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var opinia = await _context.Opinia
                .Include(o => o.OcenianyPracownik)
                .Include(o => o.Rezerwacja)
                .Include(o => o.Uzytkownik)
                .FirstOrDefaultAsync(m => m.IdOpinii == id);
            if (opinia == null)
            {
                return NotFound();
            }

            return View(opinia);
        }

        // POST: Opinia/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var opinia = await _context.Opinia.FindAsync(id);
            if (opinia != null)
            {
                _context.Opinia.Remove(opinia);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OpiniaExists(int id)
        {
            return _context.Opinia.Any(e => e.IdOpinii == id);
        }
    }
}
