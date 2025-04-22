using BookLocal.Data.Data;
using BookLocal.Data.Data.PlatformaInternetowa;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookLocal.Intranet.Controllers
{
    public class AdresController : Controller
    {
        private readonly BookLocalContext _context;

        public AdresController(BookLocalContext context)
        {
            _context = context;
        }

        // GET: Adres
        public async Task<IActionResult> Index()
        {
            var BookLocalContext = _context.Adres.Include(a => a.Pracownik).Include(a => a.Uzytkownik);
            return View(await BookLocalContext.ToListAsync());
        }

        // GET: Adres/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var adres = await _context.Adres
                .Include(a => a.Pracownik)
                .Include(a => a.Uzytkownik)
                .FirstOrDefaultAsync(m => m.IdAdresu == id);
            if (adres == null)
            {
                return NotFound();
            }

            return View(adres);
        }

        // GET: Adres/Create
        public IActionResult Create()
        {
            ViewData["PracownikId"] = new SelectList(_context.Pracownik, "IdPracownika", "Imie");
            ViewData["UzytkownikId"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email");
            return View();
        }

        // POST: Adres/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdAdresu,Ulica,NrDomu,NrLokalu,KodPocztowy,Miejscowosc,Gmina,Powiat,Wojewodztwo,Kraj,Poczta,UzytkownikId,PracownikId")] Adres adres)
        {
            if (ModelState.IsValid)
            {
                _context.Add(adres);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PracownikId"] = new SelectList(_context.Pracownik, "IdPracownika", "Imie", adres.PracownikId);
            ViewData["UzytkownikId"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", adres.UzytkownikId);
            return View(adres);
        }

        // GET: Adres/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var adres = await _context.Adres.FindAsync(id);
            if (adres == null)
            {
                return NotFound();
            }
            ViewData["PracownikId"] = new SelectList(_context.Pracownik, "IdPracownika", "Imie", adres.PracownikId);
            ViewData["UzytkownikId"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", adres.UzytkownikId);
            return View(adres);
        }

        // POST: Adres/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdAdresu,Ulica,NrDomu,NrLokalu,KodPocztowy,Miejscowosc,Gmina,Powiat,Wojewodztwo,Kraj,Poczta,UzytkownikId,PracownikId")] Adres adres)
        {
            if (id != adres.IdAdresu)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(adres);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdresExists(adres.IdAdresu))
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
            ViewData["PracownikId"] = new SelectList(_context.Pracownik, "IdPracownika", "Imie", adres.PracownikId);
            ViewData["UzytkownikId"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", adres.UzytkownikId);
            return View(adres);
        }

        // GET: Adres/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var adres = await _context.Adres
                .Include(a => a.Pracownik)
                .Include(a => a.Uzytkownik)
                .FirstOrDefaultAsync(m => m.IdAdresu == id);
            if (adres == null)
            {
                return NotFound();
            }

            return View(adres);
        }

        // POST: Adres/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var adres = await _context.Adres.FindAsync(id);
            if (adres != null)
            {
                _context.Adres.Remove(adres);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AdresExists(int id)
        {
            return _context.Adres.Any(e => e.IdAdresu == id);
        }
    }
}
