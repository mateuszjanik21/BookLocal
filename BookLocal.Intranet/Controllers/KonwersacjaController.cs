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
    public class KonwersacjaController : Controller
    {
        private readonly BookLocalContext _context;

        public KonwersacjaController(BookLocalContext context)
        {
            _context = context;
        }

        // GET: Konwersacja
        public async Task<IActionResult> Index()
        {
            var BookLocalContext = _context.Konwersacja.Include(k => k.Pracownik).Include(k => k.Uzytkownik);
            return View(await BookLocalContext.ToListAsync());
        }

        // GET: Konwersacja/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var konwersacja = await _context.Konwersacja
                .Include(k => k.Pracownik)
                .Include(k => k.Uzytkownik)
                .FirstOrDefaultAsync(m => m.IdKonwersacji == id);
            if (konwersacja == null)
            {
                return NotFound();
            }

            return View(konwersacja);
        }

        // GET: Konwersacja/Create
        public IActionResult Create()
        {
            ViewData["PracownikId"] = new SelectList(_context.Set<Pracownik>(), "IdPracownika", "Imie");
            ViewData["UzytkownikId"] = new SelectList(_context.Set<Uzytkownik>(), "IdUzytkownika", "Email");
            return View();
        }

        // POST: Konwersacja/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdKonwersacji,UzytkownikId,PracownikId,Temat,DataUtworzenia,DataOstatniejWiadomosci")] Konwersacja konwersacja)
        {
            if (ModelState.IsValid)
            {
                _context.Add(konwersacja);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PracownikId"] = new SelectList(_context.Set<Pracownik>(), "IdPracownika", "Imie", konwersacja.PracownikId);
            ViewData["UzytkownikId"] = new SelectList(_context.Set<Uzytkownik>(), "IdUzytkownika", "Email", konwersacja.UzytkownikId);
            return View(konwersacja);
        }

        // GET: Konwersacja/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var konwersacja = await _context.Konwersacja.FindAsync(id);
            if (konwersacja == null)
            {
                return NotFound();
            }
            ViewData["PracownikId"] = new SelectList(_context.Set<Pracownik>(), "IdPracownika", "Imie", konwersacja.PracownikId);
            ViewData["UzytkownikId"] = new SelectList(_context.Set<Uzytkownik>(), "IdUzytkownika", "Email", konwersacja.UzytkownikId);
            return View(konwersacja);
        }

        // POST: Konwersacja/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdKonwersacji,UzytkownikId,PracownikId,Temat,DataUtworzenia,DataOstatniejWiadomosci")] Konwersacja konwersacja)
        {
            if (id != konwersacja.IdKonwersacji)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(konwersacja);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KonwersacjaExists(konwersacja.IdKonwersacji))
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
            ViewData["PracownikId"] = new SelectList(_context.Set<Pracownik>(), "IdPracownika", "Imie", konwersacja.PracownikId);
            ViewData["UzytkownikId"] = new SelectList(_context.Set<Uzytkownik>(), "IdUzytkownika", "Email", konwersacja.UzytkownikId);
            return View(konwersacja);
        }

        // GET: Konwersacja/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var konwersacja = await _context.Konwersacja
                .Include(k => k.Pracownik)
                .Include(k => k.Uzytkownik)
                .FirstOrDefaultAsync(m => m.IdKonwersacji == id);
            if (konwersacja == null)
            {
                return NotFound();
            }

            return View(konwersacja);
        }

        // POST: Konwersacja/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var konwersacja = await _context.Konwersacja.FindAsync(id);
            if (konwersacja != null)
            {
                _context.Konwersacja.Remove(konwersacja);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KonwersacjaExists(int id)
        {
            return _context.Konwersacja.Any(e => e.IdKonwersacji == id);
        }
    }
}
