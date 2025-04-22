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
    public class PensjaController : Controller
    {
        private readonly BookLocalContext _context;

        public PensjaController(BookLocalContext context)
        {
            _context = context;
        }

        // GET: Pensja
        public async Task<IActionResult> Index()
        {
            var BookLocalContext = _context.Pensja.Include(p => p.Pracownik).Include(p => p.ZarzadzajacyPrzedsiębiorca);
            return View(await BookLocalContext.ToListAsync());
        }

        // GET: Pensja/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pensja = await _context.Pensja
                .Include(p => p.Pracownik)
                .Include(p => p.ZarzadzajacyPrzedsiębiorca)
                .FirstOrDefaultAsync(m => m.IdPensjii == id);
            if (pensja == null)
            {
                return NotFound();
            }

            return View(pensja);
        }

        // GET: Pensja/Create
        public IActionResult Create()
        {
            ViewData["PracownikId"] = new SelectList(_context.Set<Pracownik>(), "IdPracownika", "Imie");
            ViewData["ZarzadzajacyPrzedsiębiorcaId"] = new SelectList(_context.Set<Przedsiebiorca>(), "IdPrzedsiebiorcy", "Email");
            return View();
        }

        // POST: Pensja/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPensjii,PracownikId,KwotaPodstawowa,Premia,Potracenia,OkresOd,OkresDo,StatusWyplaty,DataWyplaty,Uwagi,DataUtworzeniaZapisu,ZarzadzajacyPrzedsiębiorcaId")] Pensja pensja)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pensja);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PracownikId"] = new SelectList(_context.Set<Pracownik>(), "IdPracownika", "Imie", pensja.PracownikId);
            ViewData["ZarzadzajacyPrzedsiębiorcaId"] = new SelectList(_context.Set<Przedsiebiorca>(), "IdPrzedsiebiorcy", "Email", pensja.ZarzadzajacyPrzedsiębiorcaId);
            return View(pensja);
        }

        // GET: Pensja/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pensja = await _context.Pensja.FindAsync(id);
            if (pensja == null)
            {
                return NotFound();
            }
            ViewData["PracownikId"] = new SelectList(_context.Set<Pracownik>(), "IdPracownika", "Imie", pensja.PracownikId);
            ViewData["ZarzadzajacyPrzedsiębiorcaId"] = new SelectList(_context.Set<Przedsiebiorca>(), "IdPrzedsiebiorcy", "Email", pensja.ZarzadzajacyPrzedsiębiorcaId);
            return View(pensja);
        }

        // POST: Pensja/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPensjii,PracownikId,KwotaPodstawowa,Premia,Potracenia,OkresOd,OkresDo,StatusWyplaty,DataWyplaty,Uwagi,DataUtworzeniaZapisu,ZarzadzajacyPrzedsiębiorcaId")] Pensja pensja)
        {
            if (id != pensja.IdPensjii)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pensja);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PensjaExists(pensja.IdPensjii))
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
            ViewData["PracownikId"] = new SelectList(_context.Set<Pracownik>(), "IdPracownika", "Imie", pensja.PracownikId);
            ViewData["ZarzadzajacyPrzedsiębiorcaId"] = new SelectList(_context.Set<Przedsiebiorca>(), "IdPrzedsiebiorcy", "Email", pensja.ZarzadzajacyPrzedsiębiorcaId);
            return View(pensja);
        }

        // GET: Pensja/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pensja = await _context.Pensja
                .Include(p => p.Pracownik)
                .Include(p => p.ZarzadzajacyPrzedsiębiorca)
                .FirstOrDefaultAsync(m => m.IdPensjii == id);
            if (pensja == null)
            {
                return NotFound();
            }

            return View(pensja);
        }

        // POST: Pensja/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pensja = await _context.Pensja.FindAsync(id);
            if (pensja != null)
            {
                _context.Pensja.Remove(pensja);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PensjaExists(int id)
        {
            return _context.Pensja.Any(e => e.IdPensjii == id);
        }
    }
}
