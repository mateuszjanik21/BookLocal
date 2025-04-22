using System.Diagnostics;
using BookLocal.Data.Data;
using BookLocal.Data.Data.CMS;
using BookLocal.PortalWWW.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLocal.PortalWWW.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BookLocalContext _context;

        public HomeController(ILogger<HomeController> logger, BookLocalContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.ModelSekcja = await _context.SekcjaCms
                                         .Include(s => s.PowiazaneZawartosci)
                                         .OrderBy(sekcja => sekcja.IdSekcji)
                                         .ToListAsync();

            ViewBag.ModelZawartosc = await _context.ZawartoscCms
                                             .OrderBy(zawartosc => zawartosc.IdZawartosci)
                                             .ToListAsync();

            ViewBag.ModelNaglowek = await _context.NaglowekCms
                                             .OrderBy(naglowek => naglowek.IdNapisu)
                                             .ToListAsync();

            ViewBag.ModelOdnosnika = await _context.OdnosnikCms
                                            .OrderBy(o => o.IdOdnosnika)
                                            .AsNoTracking()
                                            .ToListAsync();

            return View();
        }

        public IActionResult Uslugi()
        {
            return View();
        }

        public IActionResult Specjalisci()
        {
            return View();
        }

        public IActionResult Rezerwacje()
        {
            return View();
        }

        public IActionResult Profil()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
