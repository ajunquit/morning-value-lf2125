using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morning.Value.Web.Site.Books;
using Morning.Value.Web.Site.Home.Models;
using Morning.Value.Web.Site.Loans;
using Morning.Value.Web.Site.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace Morning.Value.Web.Site.Home.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBookRepository _books;
        private readonly LibraryService _svc;

        public HomeController(ILogger<HomeController> logger,
            IBookRepository books,
            LibraryService svc)
        {
            _logger = logger;
            this._books = books;
            this._svc = svc;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
                return View("~/Home/Views/Index.Admin.cshtml");   // Views/Home/Index.Admin.cshtml

            if (User.IsInRole("Reader")) {
                var data = await _books.GetAllAsync(); // trae todos los libros
                var vm = new ReaderHomeViewModel
                {
                    Books = data.Select(b => new BookCardViewModel
                    {
                        Id = b.Id,
                        Title = b.Title,
                        Author = b.Author,
                        Genre = b.Genre,
                        Available = b.AvailableCopies
                    }).ToList()
                };
                return View("~/Home/Views/Index.Reader.cshtml", vm);    // Views/Home/Index.User.cshtml
            }

            // Fallback por si algún usuario no tiene rol esperado
            return View("~/Home/Views/Index.Generic.cshtml");     // (opcional)
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
