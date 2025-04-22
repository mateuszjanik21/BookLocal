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
    public class PrzedsiebiorcaController : Controller
    {
        private readonly BookLocalContext _context;

        public PrzedsiebiorcaController(BookLocalContext context)
        {
            _context = context;
        }

        // GET: Przedsiebiorca
        public async Task<IActionResult> Index()
        {
            return View(await _context.Przedsiebiorca.ToListAsync());
        }

        // GET: Przedsiebiorca/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var przedsiebiorca = await _context.Przedsiebiorca
                .FirstOrDefaultAsync(m => m.IdPrzedsiebiorcy == id);
            if (przedsiebiorca == null)
            {
                return NotFound();
            }

            return View(przedsiebiorca);
        }

        // GET: Przedsiebiorca/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Przedsiebiorca/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPrzedsiebiorcy,Login,HasloHash,Email,Imie,Nazwisko,CzyAktywny")] Przedsiebiorca przedsiebiorca)
        {
            if (ModelState.IsValid)
            {
                _context.Add(przedsiebiorca);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(przedsiebiorca);
        }

        // GET: Przedsiebiorca/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var przedsiebiorca = await _context.Przedsiebiorca.FindAsync(id);
            if (przedsiebiorca == null)
            {
                return NotFound();
            }
            return View(przedsiebiorca);
        }

        // POST: Przedsiebiorca/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPrzedsiebiorcy,Login,HasloHash,Email,Imie,Nazwisko,CzyAktywny")] Przedsiebiorca przedsiebiorca)
        {
            if (id != przedsiebiorca.IdPrzedsiebiorcy)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(przedsiebiorca);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrzedsiebiorcaExists(przedsiebiorca.IdPrzedsiebiorcy))
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
            return View(przedsiebiorca);
        }

        // GET: Przedsiebiorca/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var przedsiebiorca = await _context.Przedsiebiorca
                .FirstOrDefaultAsync(m => m.IdPrzedsiebiorcy == id);
            if (przedsiebiorca == null)
            {
                return NotFound();
            }

            return View(przedsiebiorca);
        }

        // POST: Przedsiebiorca/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var przedsiebiorca = await _context.Przedsiebiorca.FindAsync(id);
            if (przedsiebiorca != null)
            {
                _context.Przedsiebiorca.Remove(przedsiebiorca);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PrzedsiebiorcaExists(int id)
        {
            return _context.Przedsiebiorca.Any(e => e.IdPrzedsiebiorcy == id);
        }
    }
}
