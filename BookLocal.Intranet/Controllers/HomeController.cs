using BookLocal.Data.Data;
using BookLocal.Data.Data.PlatformaInternetowa;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace BookLocal.Intranet.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Specjalisci()
        {
            return View();
        }

        public IActionResult Uslugi()
        {
            return View();
        }

        public IActionResult Kategorie()
        {
            return View();
        }

        public IActionResult Rezerwacje()
        {
            return View();
        }

        public IActionResult Klienci()
        {
            return View();
        }

        public IActionResult Opinie()
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
