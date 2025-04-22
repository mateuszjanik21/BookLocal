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
    public class FirmaController : Controller
    {
        private readonly BookLocalContext _context;

        public FirmaController(BookLocalContext context)
        {
            _context = context;
        }

        // GET: Firma
        public async Task<IActionResult> Index()
        {
            var BookLocalContext = _context.Firma.Include(f => f.GlownyAdres).Include(f => f.Wlasciciel);
            return View(await BookLocalContext.ToListAsync());
        }

        // GET: Firma/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var firma = await _context.Firma
                .Include(f => f.GlownyAdres)
                .Include(f => f.Wlasciciel)
                .FirstOrDefaultAsync(m => m.IdFirmy == id);
            if (firma == null)
            {
                return NotFound();
            }

            return View(firma);
        }

        // GET: Firma/Create
        public IActionResult Create()
        {
            ViewData["AdresId"] = new SelectList(_context.Adres, "IdAdresu", "Gmina");
            ViewData["WlascicielId"] = new SelectList(_context.Przedsiebiorca, "IdPrzedsiebiorcy", "Email");
            return View();
        }

        // POST: Firma/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdFirmy,Nazwa,Opis,WlascicielId,AdresId,NIP,REGON")] Firma firma)
        {
            if (ModelState.IsValid)
            {
                _context.Add(firma);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AdresId"] = new SelectList(_context.Adres, "IdAdresu", "Gmina", firma.AdresId);
            ViewData["WlascicielId"] = new SelectList(_context.Przedsiebiorca, "IdPrzedsiebiorcy", "Email", firma.WlascicielId);
            return View(firma);
        }

        // GET: Firma/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var firma = await _context.Firma.FindAsync(id);
            if (firma == null)
            {
                return NotFound();
            }
            ViewData["AdresId"] = new SelectList(_context.Adres, "IdAdresu", "Gmina", firma.AdresId);
            ViewData["WlascicielId"] = new SelectList(_context.Przedsiebiorca, "IdPrzedsiebiorcy", "Email", firma.WlascicielId);
            return View(firma);
        }

        // POST: Firma/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdFirmy,Nazwa,Opis,WlascicielId,AdresId,NIP,REGON")] Firma firma)
        {
            if (id != firma.IdFirmy)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(firma);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FirmaExists(firma.IdFirmy))
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
            ViewData["AdresId"] = new SelectList(_context.Adres, "IdAdresu", "Gmina", firma.AdresId);
            ViewData["WlascicielId"] = new SelectList(_context.Przedsiebiorca, "IdPrzedsiebiorcy", "Email", firma.WlascicielId);
            return View(firma);
        }

        // GET: Firma/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var firma = await _context.Firma
                .Include(f => f.GlownyAdres)
                .Include(f => f.Wlasciciel)
                .FirstOrDefaultAsync(m => m.IdFirmy == id);
            if (firma == null)
            {
                return NotFound();
            }

            return View(firma);
        }

        // POST: Firma/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var firma = await _context.Firma.FindAsync(id);
            if (firma != null)
            {
                _context.Firma.Remove(firma);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FirmaExists(int id)
        {
            return _context.Firma.Any(e => e.IdFirmy == id);
        }
    }
}
