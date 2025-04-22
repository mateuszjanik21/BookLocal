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
    public class ZawartoscCmsController : Controller
    {
        private readonly BookLocalContext _context;

        public ZawartoscCmsController(BookLocalContext context)
        {
            _context = context;
        }

        // GET: ZawartoscCms
        public async Task<IActionResult> Index()
        {
            var bookLocalContext = _context.ZawartoscCms.Include(z => z.Autor).Include(z => z.SekcjaPowiazana);
            return View(await bookLocalContext.ToListAsync());
        }

        // GET: ZawartoscCms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var zawartoscCms = await _context.ZawartoscCms
                .Include(z => z.Autor)
                .Include(z => z.SekcjaPowiazana)
                .FirstOrDefaultAsync(m => m.IdZawartosci == id);
            if (zawartoscCms == null)
            {
                return NotFound();
            }

            return View(zawartoscCms);
        }

        // GET: ZawartoscCms/Create
        public IActionResult Create()
        {
            ViewData["PracownikId"] = new SelectList(_context.Pracownik, "IdPracownika", "Imie");
            ViewData["SekcjaCmsId"] = new SelectList(_context.SekcjaCms, "IdSekcji", "KluczSekcji");
            return View();
        }

        // POST: ZawartoscCms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdZawartosci,Sekcja,Tresc,NazwaIkony,PracownikId,SekcjaCmsId,DataModyfikacji")] ZawartoscCms zawartoscCms)
        {
            if (ModelState.IsValid)
            {
                _context.Add(zawartoscCms);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PracownikId"] = new SelectList(_context.Pracownik, "IdPracownika", "Imie", zawartoscCms.PracownikId);
            ViewData["SekcjaCmsId"] = new SelectList(_context.SekcjaCms, "IdSekcji", "KluczSekcji", zawartoscCms.SekcjaCmsId);
            return View(zawartoscCms);
        }

        // GET: ZawartoscCms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var zawartoscCms = await _context.ZawartoscCms.FindAsync(id);
            if (zawartoscCms == null)
            {
                return NotFound();
            }
            ViewData["PracownikId"] = new SelectList(_context.Pracownik, "IdPracownika", "Imie", zawartoscCms.PracownikId);
            ViewData["SekcjaCmsId"] = new SelectList(_context.SekcjaCms, "IdSekcji", "KluczSekcji", zawartoscCms.SekcjaCmsId);
            return View(zawartoscCms);
        }

        // POST: ZawartoscCms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdZawartosci,Sekcja,Tresc,NazwaIkony,PracownikId,SekcjaCmsId,DataModyfikacji")] ZawartoscCms zawartoscCms)
        {
            if (id != zawartoscCms.IdZawartosci)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(zawartoscCms);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ZawartoscCmsExists(zawartoscCms.IdZawartosci))
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
            ViewData["PracownikId"] = new SelectList(_context.Pracownik, "IdPracownika", "Imie", zawartoscCms.PracownikId);
            ViewData["SekcjaCmsId"] = new SelectList(_context.SekcjaCms, "IdSekcji", "KluczSekcji", zawartoscCms.SekcjaCmsId);
            return View(zawartoscCms);
        }

        // GET: ZawartoscCms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var zawartoscCms = await _context.ZawartoscCms
                .Include(z => z.Autor)
                .Include(z => z.SekcjaPowiazana)
                .FirstOrDefaultAsync(m => m.IdZawartosci == id);
            if (zawartoscCms == null)
            {
                return NotFound();
            }

            return View(zawartoscCms);
        }

        // POST: ZawartoscCms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var zawartoscCms = await _context.ZawartoscCms.FindAsync(id);
            if (zawartoscCms != null)
            {
                _context.ZawartoscCms.Remove(zawartoscCms);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ZawartoscCmsExists(int id)
        {
            return _context.ZawartoscCms.Any(e => e.IdZawartosci == id);
        }
    }
}
