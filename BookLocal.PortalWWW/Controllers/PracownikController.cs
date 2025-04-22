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
    public class PracownikController : Controller
    {
        private readonly BookLocalContext _context;

        public PracownikController(BookLocalContext context)
        {
            _context = context;
        }

        // GET: Pracownik
        public async Task<IActionResult> Index(string? searchName, int? searchServiceId)
        {

            ViewBag.ModelOdnosnika = await _context.OdnosnikCms
                                            .OrderBy(o => o.IdOdnosnika)
                                            .AsNoTracking()
                                            .ToListAsync();

            await PopulateUslugiDropDownList(searchServiceId);
            ViewData["CurrentFilterName"] = searchName;

            var query = _context.Pracownik
                            .Where(p => p.CzyAktywny);

            // Filtrowanie po imieniu/nazwisku
            if (!string.IsNullOrEmpty(searchName))
            {
                string searchTerm = searchName.ToLower().Trim();
                query = query.Where(p => (p.Imie + " " + p.Nazwisko).ToLower().Contains(searchTerm) ||
                                         (p.Nazwisko + " " + p.Imie).ToLower().Contains(searchTerm));
            }

            // Filtrowanie po ID usługi bazowej
            if (searchServiceId.HasValue && searchServiceId > 0)
            {
                query = query.Where(p => p.OferowaneUslugiSzczegoly.Any(sz => sz.UslugaId == searchServiceId.Value));
            }

            var specjalisci = await query
                                    .OrderBy(p => p.Nazwisko).ThenBy(p => p.Imie)
                                    .AsNoTracking()
                                    .ToListAsync();

            return View(specjalisci);
        }

        private async Task PopulateUslugiDropDownList(object? selectedUsluga = null)
        {
            var uslugi = await _context.Usluga 
                               .OrderBy(u => u.Nazwa)
                               .Select(u => new { u.IdUslugi, u.Nazwa })
                               .ToListAsync();
            ViewBag.UslugaList = new SelectList(uslugi, "IdUslugi", "Nazwa", selectedUsluga);
            ViewData["CurrentFilterServiceId"] = selectedUsluga;
        }

        // GET: Pracownik/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pracownik = await _context.Pracownik
                .Include(p => p.Firma)
                .FirstOrDefaultAsync(m => m.IdPracownika == id);
            if (pracownik == null)
            {
                return NotFound();
            }

            return View(pracownik);
        }

        // GET: Pracownik/Create
        public IActionResult Create()
        {
            ViewData["FirmaId"] = new SelectList(_context.Firma, "IdFirmy", "Nazwa");
            return View();
        }

        // POST: Pracownik/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPracownika,Imie,Nazwisko,Bio,ZdjecieUrl,Stanowisko,EmailKontaktowy,TelefonKontaktowy,FirmaId,CzyAktywny")] Pracownik pracownik)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pracownik);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FirmaId"] = new SelectList(_context.Firma, "IdFirmy", "Nazwa", pracownik.FirmaId);
            return View(pracownik);
        }

        // GET: Pracownik/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pracownik = await _context.Pracownik.FindAsync(id);
            if (pracownik == null)
            {
                return NotFound();
            }
            ViewData["FirmaId"] = new SelectList(_context.Firma, "IdFirmy", "Nazwa", pracownik.FirmaId);
            return View(pracownik);
        }

        // POST: Pracownik/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPracownika,Imie,Nazwisko,Bio,ZdjecieUrl,Stanowisko,EmailKontaktowy,TelefonKontaktowy,FirmaId,CzyAktywny")] Pracownik pracownik)
        {
            if (id != pracownik.IdPracownika)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pracownik);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PracownikExists(pracownik.IdPracownika))
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
            ViewData["FirmaId"] = new SelectList(_context.Firma, "IdFirmy", "Nazwa", pracownik.FirmaId);
            return View(pracownik);
        }

        // GET: Pracownik/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pracownik = await _context.Pracownik
                .Include(p => p.Firma)
                .FirstOrDefaultAsync(m => m.IdPracownika == id);
            if (pracownik == null)
            {
                return NotFound();
            }

            return View(pracownik);
        }

        // POST: Pracownik/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pracownik = await _context.Pracownik.FindAsync(id);
            if (pracownik != null)
            {
                _context.Pracownik.Remove(pracownik);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PracownikExists(int id)
        {
            return _context.Pracownik.Any(e => e.IdPracownika == id);
        }
    }
}
