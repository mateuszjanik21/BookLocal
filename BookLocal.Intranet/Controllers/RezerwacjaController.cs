using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookLocal.Data.Data;
using BookLocal.Data.Data.PlatformaInternetowa;
using Microsoft.AspNetCore.Authorization;

namespace BookLocal.Data.Controllers
{
    public class RezerwacjaController : Controller
    {
        private readonly BookLocalContext _context;

        public RezerwacjaController(BookLocalContext context)
        {
            _context = context;
        }

        public async Task<JsonResult> GetSzczegolyUslugiForPracownik(int pracownikId)
        {
            if (pracownikId <= 0)
            {
                return Json(new List<object>());
            }

            try
            {
                var uslugi = await _context.SzczegolyUslugi
                                     .Include(sz => sz.Usluga) 
                                     .Where(sz => sz.PracownikId == pracownikId)
                                     .OrderBy(sz => sz.Usluga.Nazwa).ThenBy(sz => sz.Opis)
                                     .Select(sz => new
                                     {
                                         value = sz.IdSzczegolowUslugi.ToString(), 
                                         text = $"{sz.Usluga.Nazwa} ({sz.Opis}) - {sz.Cena.ToString("N2")} zł" 
                                     })
                                     .ToListAsync();

                return Json(uslugi); 
            }
            catch (Exception ex)
            {
                return Json(new { error = "Wystąpił błąd podczas pobierania usług." });
            }
        }

        public async Task<IActionResult> GetBookingsForCalendar(DateTime? start, DateTime? end)
        {
            var startDate = start?.ToUniversalTime() ?? DateTime.UtcNow.AddMonths(-1);
            var endDate = end?.ToUniversalTime() ?? DateTime.UtcNow.AddMonths(1);

            try
            {
                var rezerwacje = await _context.Rezerwacja
                    .Include(r => r.Uzytkownik)
                    .Include(r => r.WykonujacyPracownik)
                    .Include(r => r.SzczegolyUslugi).ThenInclude(sz => sz.Usluga) 
                    .Where(r => r.DataRezerwacji >= startDate &&
                                r.DataRezerwacji < endDate &&
                                r.Status != "Anulowana") 
                    .ToListAsync();

                var events = rezerwacje.Select(r => new
                {
                    id = r.IdRezerwacji.ToString(),
                    title = $"{(r.UzytkownikId.HasValue ? (r.Uzytkownik?.Imie ?? "") + " " + (r.Uzytkownik?.Nazwisko ?? "") : (r.ImieKlienta ?? "") + " " + (r.NazwiskoKlienta ?? ""))} - {r.SzczegolyUslugi?.Usluga?.Nazwa ?? "Usługa"}",
                    start = r.DataRezerwacji.ToString("o"),

                    end = r.SzczegolyUslugi != null && r.SzczegolyUslugi.CzasTrwaniaMinuty > 0
                          ? r.DataRezerwacji.AddMinutes(r.SzczegolyUslugi.CzasTrwaniaMinuty).ToString("o")
                          : r.DataRezerwacji.AddHours(1).ToString("o"),

                    extendedProps = new
                    {
                        description = $"Pracownik: {r.WykonujacyPracownik?.Imie ?? ""} {r.WykonujacyPracownik?.Nazwisko ?? ""}\nUsługa: {r.SzczegolyUslugi?.Opis ?? ""}\nStatus: {r.Status ?? ""}"
                    }
                }).ToList();

                return Json(events);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Błąd pobierania rezerwacji." });
            }
        }

        // GET: Rezerwacja
        public async Task<IActionResult> Index()
        {
            try
            {
                var teraz = DateTime.UtcNow;
                var rezerwacjeDoAktualizacji = await _context.Rezerwacja
                    .Include(r => r.SzczegolyUslugi) 
                    .Where(r => r.SzczegolyUslugi != null && r.WykonujacyPracownikId != null &&
                                (r.Status == "Potwierdzona" || r.Status == "Oczekująca") &&
                                (r.DataRezerwacji.AddMinutes(r.SzczegolyUslugi.CzasTrwaniaMinuty) < teraz)
                          )
                    .ToListAsync();

                if (rezerwacjeDoAktualizacji.Any())
                {
                    foreach (var rezerwacja in rezerwacjeDoAktualizacji)
                    {
                        rezerwacja.Status = "Zakończona";
                        _context.Update(rezerwacja);
                    }
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Wystąpił błąd podczas automatycznej aktualizacji statusów rezerwacji.";
            }
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            ViewData["StatTotal"] = await _context.Rezerwacja.Where(r => r.DataRezerwacji >= thirtyDaysAgo).CountAsync();
            ViewData["StatOczekujace"] = await _context.Rezerwacja.Where(r => r.DataRezerwacji >= thirtyDaysAgo && r.Status == "Oczekująca").CountAsync();
            ViewData["StatPotwierdzone"] = await _context.Rezerwacja.Where(r => r.DataRezerwacji >= thirtyDaysAgo && r.Status == "Potwierdzona").CountAsync();
            ViewData["StatZakonczone"] = await _context.Rezerwacja.Where(r => r.DataRezerwacji >= thirtyDaysAgo && r.Status == "Zakończona").CountAsync();
            ViewData["StatAnulowane"] = await _context.Rezerwacja.Where(r => r.DataRezerwacji >= thirtyDaysAgo && r.Status == "Anulowana").CountAsync();
            var rezerwacjeDoTabeli = await _context.Rezerwacja
                                        .Include(r => r.Uzytkownik)
                                        .Include(r => r.WykonujacyPracownik)
                                        .Include(r => r.SzczegolyUslugi).ThenInclude(sz => sz.Usluga)
                                        .OrderByDescending(r => r.DataRezerwacji)
                                        .Take(50)
                                        .AsNoTracking() 
                                        .ToListAsync();

            return View(rezerwacjeDoTabeli);
        }

        // GET: Rezerwacja/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezerwacja = await _context.Rezerwacja
                .Include(r => r.ObslugujacyPrzedsiębiorca)
                .Include(r => r.SzczegolyUslugi)
                .Include(r => r.Uzytkownik)
                .Include(r => r.WykonujacyPracownik)
                .FirstOrDefaultAsync(m => m.IdRezerwacji == id);
            if (rezerwacja == null)
            {
                return NotFound();
            }

            return View(rezerwacja);
        }

        // GET: Rezerwacja/Create
        public IActionResult Create()
        {
            ViewData["ObslugujacyPrzedsiębiorcaId"] = new SelectList(_context.Przedsiebiorca, "IdPrzedsiebiorcy", "Email");
            ViewData["SzczegolyUslugiId"] = new SelectList(_context.SzczegolyUslugi, "IdSzczegolowUslugi", "Opis");
            ViewData["UzytkownikId"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email");
            ViewData["WykonujacyPracownikId"] = new SelectList(_context.Pracownik, "IdPracownika", "Imie");
            return View();
        }

        // POST: Rezerwacja/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdRezerwacji,DataRezerwacji,Status,ImieKlienta,NazwiskoKlienta,TelefonKlienta,UzytkownikId,WykonujacyPracownikId,ObslugujacyPrzedsiębiorcaId,SzczegolyUslugiId")] Rezerwacja rezerwacja)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rezerwacja);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ObslugujacyPrzedsiębiorcaId"] = new SelectList(_context.Przedsiebiorca, "IdPrzedsiebiorcy", "Email", rezerwacja.ObslugujacyPrzedsiębiorcaId);
            ViewData["SzczegolyUslugiId"] = new SelectList(_context.SzczegolyUslugi, "IdSzczegolowUslugi", "Opis", rezerwacja.SzczegolyUslugiId);
            ViewData["UzytkownikId"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", rezerwacja.UzytkownikId);
            ViewData["WykonujacyPracownikId"] = new SelectList(_context.Pracownik, "IdPracownika", "Imie", rezerwacja.WykonujacyPracownikId);
            return View(rezerwacja);
        }

        // GET: Rezerwacja/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezerwacja = await _context.Rezerwacja 
                .Include(r => r.SzczegolyUslugi) 
                    .ThenInclude(sz => sz.Usluga) 
                .Include(r => r.Uzytkownik) 
                .Include(r => r.WykonujacyPracownik)  
                .FirstOrDefaultAsync(r => r.IdRezerwacji == id);
            if (rezerwacja == null)
            {
                return NotFound();
            }
            ViewData["ObslugujacyPrzedsiębiorcaId"] = new SelectList(_context.Przedsiebiorca, "IdPrzedsiebiorcy", "Email", rezerwacja.ObslugujacyPrzedsiębiorcaId);
            ViewData["SzczegolyUslugiId"] = new SelectList(_context.SzczegolyUslugi, "IdSzczegolowUslugi", "Opis", rezerwacja.SzczegolyUslugiId);
            ViewData["UzytkownikId"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", rezerwacja.UzytkownikId);
            ViewData["WykonujacyPracownikId"] = new SelectList(_context.Pracownik, "IdPracownika", "Imie", rezerwacja.WykonujacyPracownikId);
            return View(rezerwacja);
        }

        // POST: Rezerwacja/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdRezerwacji,DataRezerwacji,Status,ImieKlienta,NazwiskoKlienta,TelefonKlienta,UzytkownikId,WykonujacyPracownikId,ObslugujacyPrzedsiębiorcaId,SzczegolyUslugiId")] Rezerwacja rezerwacja)
        {
            if (id != rezerwacja.IdRezerwacji)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rezerwacja);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RezerwacjaExists(rezerwacja.IdRezerwacji))
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
            ViewData["ObslugujacyPrzedsiębiorcaId"] = new SelectList(_context.Przedsiebiorca, "IdPrzedsiebiorcy", "Email", rezerwacja.ObslugujacyPrzedsiębiorcaId);
            ViewData["SzczegolyUslugiId"] = new SelectList(_context.SzczegolyUslugi, "IdSzczegolowUslugi", "Opis", rezerwacja.SzczegolyUslugiId);
            ViewData["UzytkownikId"] = new SelectList(_context.Uzytkownik, "IdUzytkownika", "Email", rezerwacja.UzytkownikId);
            ViewData["WykonujacyPracownikId"] = new SelectList(_context.Pracownik, "IdPracownika", "Imie", rezerwacja.WykonujacyPracownikId);
            return View(rezerwacja);
        }

        // GET: Rezerwacja/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezerwacja = await _context.Rezerwacja
                .Include(r => r.ObslugujacyPrzedsiębiorca)
                .Include(r => r.SzczegolyUslugi)
                .Include(r => r.Uzytkownik)
                .Include(r => r.WykonujacyPracownik)
                .FirstOrDefaultAsync(m => m.IdRezerwacji == id);
            if (rezerwacja == null)
            {
                return NotFound();
            }

            return View(rezerwacja);
        }

        // POST: Rezerwacja/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rezerwacja = await _context.Rezerwacja.FindAsync(id);
            if (rezerwacja != null)
            {
                _context.Rezerwacja.Remove(rezerwacja);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RezerwacjaExists(int id)
        {
            return _context.Rezerwacja.Any(e => e.IdRezerwacji == id);
        }
    }
}
