using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookLocal.Data.Data;
using BookLocal.Data.Data.PlatformaInternetowa;
using BookLocal.PortalWWW.Models.ViewModel;

namespace BookLocal.PortalWWW.Controllers
{
    public class RezerwacjaController : Controller
    {
        private readonly BookLocalContext _context;

        public RezerwacjaController(BookLocalContext context )
        {
            _context = context;
        }

        // GET: Rezerwacja (Bez zmian z Twojego kodu)
        public async Task<IActionResult> Index()
        {
            ViewBag.ModelOdnosnika = await _context.OdnosnikCms
                                .OrderBy(o => o.IdOdnosnika)
                                .AsNoTracking()
                                .ToListAsync();
            var bookLocalContext = _context.Rezerwacja
                                        .Include(r => r.ObslugujacyPrzedsiębiorca)
                                        .Include(r => r.SzczegolyUslugi)
                                            .ThenInclude(sz => sz.Usluga) 
                                        .Include(r => r.Uzytkownik)
                                        .Include(r => r.WykonujacyPracownik)
                                        .AsNoTracking(); 
            return View(await bookLocalContext.ToListAsync());
        }

        // GET: Rezerwacja/Details/5 (Bez zmian z Twojego kodu)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezerwacja = await _context.Rezerwacja
                .Include(r => r.ObslugujacyPrzedsiębiorca)
                .Include(r => r.SzczegolyUslugi)
                     .ThenInclude(sz => sz.Usluga)
                .Include(r => r.Uzytkownik)
                .Include(r => r.WykonujacyPracownik)
                .AsNoTracking() 
                .FirstOrDefaultAsync(m => m.IdRezerwacji == id);
            if (rezerwacja == null)
            {
                return NotFound();
            }

            return View(rezerwacja);
        }

        // GET: Rezerwacja/Create
        public async Task<IActionResult> Create(int? szczegolyUslugiId, int? pracownikId)
        {
            if (szczegolyUslugiId == null || pracownikId == null)
            {
                return BadRequest("Brak informacji o usłudze lub specjaliście do zarezerwowania.");
            }

            var szczegoly = await _context.SzczegolyUslugi
                .Include(sz => sz.Usluga)
                .AsNoTracking()
                .FirstOrDefaultAsync(sz => sz.IdSzczegolowUslugi == szczegolyUslugiId.Value);

            var pracownik = await _context.Pracownik
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdPracownika == pracownikId.Value);

            if (szczegoly == null || pracownik == null || szczegoly.Usluga == null)
            {
                return NotFound("Nie znaleziono podanej usługi lub specjalisty.");
            }

            if (szczegoly.PracownikId != pracownik.IdPracownika)
            {
                return BadRequest("Niezgodność danych specjalisty i usługi.");
            }

            var viewModel = new RezerwacjaCreateViewModel
            {
                SzczegolyUslugiId = szczegoly.IdSzczegolowUslugi,
                PracownikId = pracownik.IdPracownika,
                NazwaUslugi = szczegoly.Usluga.Nazwa,
                OpisSzczegolowUslugi = szczegoly.Opis,
                Cena = szczegoly.Cena,
                CzasTrwaniaMinuty = szczegoly.CzasTrwaniaMinuty,
                ImieNazwiskoPracownika = $"{pracownik.Imie} {pracownik.Nazwisko}",
                ZdjeciePracownikaUrl = pracownik.ZdjecieUrl,
                StanowiskoPracownika = pracownik.Stanowisko,
                WybranaData = DateTime.Today.AddDays(1) 
                
            };

            return View(viewModel);
        }


        // POST: Rezerwacja/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RezerwacjaCreateViewModel viewModel)
        {
            DateTime? proponowanyStart = null;
            if (viewModel.WybranaData.HasValue && viewModel.WybranaGodzina.HasValue)
            {
                proponowanyStart = viewModel.WybranaData.Value.Date + viewModel.WybranaGodzina.Value;
            }

            var szczegoly = await _context.SzczegolyUslugi
                                      .Include(sz => sz.Usluga)
                                      .AsNoTracking()
                                      .FirstOrDefaultAsync(sz => sz.IdSzczegolowUslugi == viewModel.SzczegolyUslugiId);

            if (szczegoly == null)
            {
                ModelState.AddModelError(string.Empty, "Wystąpił błąd podczas przetwarzania usługi.");
                return View(viewModel); 
            }
            viewModel.NazwaUslugi = szczegoly.Usluga?.Nazwa;
            viewModel.CzasTrwaniaMinuty = szczegoly.CzasTrwaniaMinuty;

            if (!proponowanyStart.HasValue)
            {
                TimeSpan minTime = new TimeSpan(8, 0, 0);
                TimeSpan maxTime = new TimeSpan(18, 0, 0); 

                if (!viewModel.WybranaData.HasValue) ModelState.AddModelError(nameof(viewModel.WybranaData), "Proszę wybrać datę wizyty.");
                if (!viewModel.WybranaGodzina.HasValue) ModelState.AddModelError(nameof(viewModel.WybranaGodzina), "Proszę wybrać godzinę wizyty.");

                if (viewModel.WybranaGodzina.Value < minTime)
                {
                    ModelState.AddModelError(nameof(viewModel.WybranaGodzina), $"Godzina wizyty nie może być wcześniejsza niż {minTime:hh\\:mm}.");
                }
                else if (viewModel.WybranaGodzina.Value > maxTime)
                {
                    ModelState.AddModelError(nameof(viewModel.WybranaGodzina), $"Godzina wizyty nie może być późniejsza niż {maxTime:hh\\:mm}.");
                }
            }
            else if (proponowanyStart.Value <= DateTime.Now)
            {
                ModelState.AddModelError(nameof(viewModel.WybranaData), "Nie można rezerwować terminów w przeszłości.");
            }

            if (proponowanyStart.HasValue && proponowanyStart.Value > DateTime.Now)
            {
                DateTime proponowanyKoniec = proponowanyStart.Value.AddMinutes(szczegoly.CzasTrwaniaMinuty);

                var existingReservations = await _context.Rezerwacja
                    .Include(r => r.SzczegolyUslugi) 
                    .Where(r => r.WykonujacyPracownikId == viewModel.PracownikId &&
                                (r.Status == "Oczekująca" || r.Status == "Potwierdzona"))
                    .Select(r => new { r.DataRezerwacji, r.SzczegolyUslugi.CzasTrwaniaMinuty }) 
                    .ToListAsync();

                bool isConflict = existingReservations.Any(r =>
                                    proponowanyStart < r.DataRezerwacji.AddMinutes(r.CzasTrwaniaMinuty) && 
                                    proponowanyKoniec > r.DataRezerwacji);

                if (isConflict)
                {
                    ModelState.AddModelError(string.Empty, $"Wybrany termin ({proponowanyStart:dd.MM.yyyy HH:mm}) jest już zajęty lub koliduje. Proszę wybrać inny.");
                }
            }


            if (ModelState.IsValid)
            {
                var nowaRezerwacja = new Rezerwacja
                {
                    DataRezerwacji = proponowanyStart!.Value, 
                    Status = "Oczekująca", 

                    ImieKlienta = viewModel.ImieKlienta,
                    NazwiskoKlienta = viewModel.NazwiskoKlienta,
                    TelefonKlienta = viewModel.TelefonKlienta,
                    WykonujacyPracownikId = viewModel.PracownikId,
                    SzczegolyUslugiId = viewModel.SzczegolyUslugiId,
                };

                _context.Rezerwacja.Add(nowaRezerwacja);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Confirmation), new { id = nowaRezerwacja.IdRezerwacji });
            }

            var pracownik = await _context.Pracownik.AsNoTracking().FirstOrDefaultAsync(p => p.IdPracownika == viewModel.PracownikId);
            if (pracownik != null)
            {
                viewModel.ImieNazwiskoPracownika = $"{pracownik.Imie} {pracownik.Nazwisko}";
                viewModel.ZdjeciePracownikaUrl = pracownik.ZdjecieUrl;
                viewModel.StanowiskoPracownika = pracownik.Stanowisko;
            }
            viewModel.Cena = szczegoly.Cena; 

            return View(viewModel);
        }


        // GET: Rezerwacja/Confirmation/{id}
        public async Task<IActionResult> Confirmation(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezerwacja = await _context.Rezerwacja
                .Include(r => r.WykonujacyPracownik)
                .Include(r => r.SzczegolyUslugi)
                    .ThenInclude(sz => sz.Usluga)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdRezerwacji == id);

            if (rezerwacja == null)
            {
                return NotFound();
            }

            return View(rezerwacja);
        }


        // GET: Rezerwacja/Edit/5 (Bez zmian z Twojego kodu)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezerwacja = await _context.Rezerwacja
                .Include(r => r.SzczegolyUslugi)
                    .ThenInclude(sz => sz.Usluga) 
                .Include(r => r.WykonujacyPracownik) 
                .Include(r => r.Uzytkownik) 
                .AsNoTracking() 
                .FirstOrDefaultAsync(m => m.IdRezerwacji == id);

            if (rezerwacja == null)
            {
                return NotFound();
            }
            return View(rezerwacja);
        }

        // POST: Rezerwacja/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdRezerwacji,DataRezerwacji,Status,ImieKlienta,NazwiskoKlienta,TelefonKlienta,UzytkownikId,WykonujacyPracownikId,ObslugujacyPrzedsiębiorcaId,SzczegolyUslugiId")] Rezerwacja rezerwacjaViewModel)
        {
            if (id != rezerwacjaViewModel.IdRezerwacji)
            {
                return NotFound("Niezgodność identyfikatorów.");
            }

            if (rezerwacjaViewModel.DataRezerwacji <= DateTime.Now)
            {
                ModelState.AddModelError(nameof(Rezerwacja.DataRezerwacji), "Nie można przełożyć rezerwacji na termin w przeszłości.");
            }

            var rezerwacjaToUpdate = await _context.Rezerwacja
                                        .Include(r => r.SzczegolyUslugi) 
                                        .FirstOrDefaultAsync(r => r.IdRezerwacji == id);

            if (rezerwacjaToUpdate == null)
            {
                return NotFound("Nie znaleziono rezerwacji do aktualizacji.");
            }

            if (rezerwacjaToUpdate.SzczegolyUslugi == null)
            {
                ModelState.AddModelError(string.Empty, "Błąd: Nie można pobrać szczegółów usługi dla tej rezerwacji. Edycja niemożliwa.");
                
                await ReloadIncludesForView(rezerwacjaViewModel);
                return View(rezerwacjaViewModel);
            }


            if (ModelState.IsValid && rezerwacjaViewModel.DataRezerwacji > DateTime.Now)
            {
                DateTime proponowanyStart = rezerwacjaViewModel.DataRezerwacji;
                DateTime proponowanyKoniec = proponowanyStart.AddMinutes(rezerwacjaToUpdate.SzczegolyUslugi.CzasTrwaniaMinuty);

                var existingReservations = await _context.Rezerwacja
                    .Include(r => r.SzczegolyUslugi) 
                    .Where(r => r.WykonujacyPracownikId == rezerwacjaToUpdate.WykonujacyPracownikId && 
                                r.IdRezerwacji != id &&
                               (r.Status == "Oczekująca" || r.Status == "Potwierdzona"))
                    .Select(r => new { r.DataRezerwacji, r.SzczegolyUslugi.CzasTrwaniaMinuty })
                    .ToListAsync();

                bool isConflict = existingReservations.Any(r =>
                    proponowanyStart < r.DataRezerwacji.AddMinutes(r.CzasTrwaniaMinuty) &&
                    proponowanyKoniec > r.DataRezerwacji);
                if (isConflict)
                {
                    ModelState.AddModelError(string.Empty, $"Wybrany termin ({proponowanyStart:dd.MM.yyyy HH:mm}) koliduje z inną rezerwacją tego specjalisty. Proszę wybrać inny.");
                }
            }

            if (ModelState.IsValid) 
            {
                try
                {
                    rezerwacjaToUpdate.DataRezerwacji = rezerwacjaViewModel.DataRezerwacji;

                    _context.Update(rezerwacjaToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RezerwacjaExists(rezerwacjaViewModel.IdRezerwacji))
                    {
                        return NotFound("W międzyczasie rezerwacja została usunięta.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Wystąpił konflikt podczas zapisu. Dane mogły zostać zmienione przez innego użytkownika. Spróbuj ponownie.");
                        await ReloadIncludesForView(rezerwacjaViewModel);
                        return View(rezerwacjaViewModel);
                    }
                }
                TempData["SuccessMessage"] = "Termin rezerwacji został pomyślnie zmieniony.";
                return RedirectToAction(nameof(Index));
            }
            await ReloadIncludesForView(rezerwacjaViewModel);
            return View(rezerwacjaViewModel);
        }


        // GET: Rezerwacja/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezerwacja = await _context.Rezerwacja
                .Include(r => r.SzczegolyUslugi)
                    .ThenInclude(sz => sz.Usluga)
                .Include(r => r.WykonujacyPracownik)
                .AsNoTracking() 
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
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Rezerwacja została pomyślnie usunięta.";
            }
            else
            {
                TempData["ErrorMessage"] = "Nie znaleziono rezerwacji do usunięcia.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task ReloadIncludesForView(Rezerwacja rezerwacja)
        {
            var details = await _context.SzczegolyUslugi.Include(sz => sz.Usluga).AsNoTracking().FirstOrDefaultAsync(sz => sz.IdSzczegolowUslugi == rezerwacja.SzczegolyUslugiId);
            var pracownik = await _context.Pracownik.AsNoTracking().FirstOrDefaultAsync(p => p.IdPracownika == rezerwacja.WykonujacyPracownikId);
            Uzytkownik uzytkownik = null;
            if (rezerwacja.UzytkownikId.HasValue)
            {
                uzytkownik = await _context.Uzytkownik.AsNoTracking().FirstOrDefaultAsync(u => u.IdUzytkownika == rezerwacja.UzytkownikId);
            }

            if (details != null) rezerwacja.SzczegolyUslugi = details;
            if (pracownik != null) rezerwacja.WykonujacyPracownik = pracownik;
            if (uzytkownik != null) rezerwacja.Uzytkownik = uzytkownik;
        }


        private bool RezerwacjaExists(int id)
        {
            return _context.Rezerwacja.Any(e => e.IdRezerwacji == id);
        }
    }
}