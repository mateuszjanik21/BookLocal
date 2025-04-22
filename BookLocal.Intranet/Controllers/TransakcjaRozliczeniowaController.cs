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
    public class TransakcjaRozliczeniowaController : Controller
    {
        private readonly BookLocalContext _context;

        public TransakcjaRozliczeniowaController(BookLocalContext context)
        {
            _context = context;
        }

        // GET: TransakcjaRozliczeniowa
        public async Task<IActionResult> Index()
        {
            var BookLocalContext = _context.TransakcjaRozliczeniowa.Include(t => t.Pracownik).Include(t => t.Rezerwacja).Include(t => t.ZatwierdzajacyPrzedsiębiorca);
            return View(await BookLocalContext.ToListAsync());
        }

        // GET: TransakcjaRozliczeniowa/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transakcjaRozliczeniowa = await _context.TransakcjaRozliczeniowa
                .Include(t => t.Pracownik)
                .Include(t => t.Rezerwacja)
                .Include(t => t.ZatwierdzajacyPrzedsiębiorca)
                .FirstOrDefaultAsync(m => m.IdTransakcji == id);
            if (transakcjaRozliczeniowa == null)
            {
                return NotFound();
            }

            return View(transakcjaRozliczeniowa);
        }

        // GET: TransakcjaRozliczeniowa/Create
        public IActionResult Create()
        {
            ViewData["PracownikId"] = new SelectList(_context.Pracownik, "IdPracownika", "Imie");
            ViewData["RezerwacjaId"] = new SelectList(_context.Rezerwacja, "IdRezerwacji", "Status");
            ViewData["ZatwierdzajacyPrzedsiębiorcaId"] = new SelectList(_context.Przedsiebiorca, "IdPrzedsiebiorcy", "Email");
            return View();
        }

        // POST: TransakcjaRozliczeniowa/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdTransakcji,PracownikId,RezerwacjaId,KwotaBrutto,ProwizjaPlatformy,ProwizjaFirmy,KwotaNettoDlaPracownika,StatusRozliczenia,DataUtworzenia,DataOstatniejZmianyStatusu,Uwagi,ZatwierdzajacyPrzedsiębiorcaId")] TransakcjaRozliczeniowa transakcjaRozliczeniowa)
        {
            if (ModelState.IsValid)
            {
                _context.Add(transakcjaRozliczeniowa);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PracownikId"] = new SelectList(_context.Pracownik, "IdPracownika", "Imie", transakcjaRozliczeniowa.PracownikId);
            ViewData["RezerwacjaId"] = new SelectList(_context.Rezerwacja, "IdRezerwacji", "Status", transakcjaRozliczeniowa.RezerwacjaId);
            ViewData["ZatwierdzajacyPrzedsiębiorcaId"] = new SelectList(_context.Przedsiebiorca, "IdPrzedsiebiorcy", "Email", transakcjaRozliczeniowa.ZatwierdzajacyPrzedsiębiorcaId);
            return View(transakcjaRozliczeniowa);
        }

        // GET: TransakcjaRozliczeniowa/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transakcjaRozliczeniowa = await _context.TransakcjaRozliczeniowa.FindAsync(id);
            if (transakcjaRozliczeniowa == null)
            {
                return NotFound();
            }
            ViewData["PracownikId"] = new SelectList(_context.Pracownik, "IdPracownika", "Imie", transakcjaRozliczeniowa.PracownikId);
            ViewData["RezerwacjaId"] = new SelectList(_context.Rezerwacja, "IdRezerwacji", "Status", transakcjaRozliczeniowa.RezerwacjaId);
            ViewData["ZatwierdzajacyPrzedsiębiorcaId"] = new SelectList(_context.Przedsiebiorca, "IdPrzedsiebiorcy", "Email", transakcjaRozliczeniowa.ZatwierdzajacyPrzedsiębiorcaId);
            return View(transakcjaRozliczeniowa);
        }

        // POST: TransakcjaRozliczeniowa/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdTransakcji,PracownikId,RezerwacjaId,KwotaBrutto,ProwizjaPlatformy,ProwizjaFirmy,KwotaNettoDlaPracownika,StatusRozliczenia,DataUtworzenia,DataOstatniejZmianyStatusu,Uwagi,ZatwierdzajacyPrzedsiębiorcaId")] TransakcjaRozliczeniowa transakcjaRozliczeniowa)
        {
            if (id != transakcjaRozliczeniowa.IdTransakcji)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transakcjaRozliczeniowa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransakcjaRozliczeniowaExists(transakcjaRozliczeniowa.IdTransakcji))
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
            ViewData["PracownikId"] = new SelectList(_context.Pracownik, "IdPracownika", "Imie", transakcjaRozliczeniowa.PracownikId);
            ViewData["RezerwacjaId"] = new SelectList(_context.Rezerwacja, "IdRezerwacji", "Status", transakcjaRozliczeniowa.RezerwacjaId);
            ViewData["ZatwierdzajacyPrzedsiębiorcaId"] = new SelectList(_context.Przedsiebiorca, "IdPrzedsiebiorcy", "Email", transakcjaRozliczeniowa.ZatwierdzajacyPrzedsiębiorcaId);
            return View(transakcjaRozliczeniowa);
        }

        // GET: TransakcjaRozliczeniowa/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transakcjaRozliczeniowa = await _context.TransakcjaRozliczeniowa
                .Include(t => t.Pracownik)
                .Include(t => t.Rezerwacja)
                .Include(t => t.ZatwierdzajacyPrzedsiębiorca)
                .FirstOrDefaultAsync(m => m.IdTransakcji == id);
            if (transakcjaRozliczeniowa == null)
            {
                return NotFound();
            }

            return View(transakcjaRozliczeniowa);
        }

        // POST: TransakcjaRozliczeniowa/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transakcjaRozliczeniowa = await _context.TransakcjaRozliczeniowa.FindAsync(id);
            if (transakcjaRozliczeniowa != null)
            {
                _context.TransakcjaRozliczeniowa.Remove(transakcjaRozliczeniowa);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransakcjaRozliczeniowaExists(int id)
        {
            return _context.TransakcjaRozliczeniowa.Any(e => e.IdTransakcji == id);
        }
    }
}
