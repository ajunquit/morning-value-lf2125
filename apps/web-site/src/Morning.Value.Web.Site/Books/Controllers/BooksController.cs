using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Morning.Value.Web.Site.Books.Models;
using Morning.Value.Web.Site.Common.Models;
using Morning.Value.Web.Site.Home.Controllers;
using Morning.Value.Web.Site.Loans;
using Morning.Value.Web.Site.Loans.Enums;
using Morning.Value.Web.Site.Loans.Models;
using System.Security.Claims;
using static Morning.Value.Web.Site.Books.BookRepository;

namespace Morning.Value.Web.Site.Books.Controllers
{
    [Authorize]
    public class BooksController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ILoanRepository _loans;
        private readonly IBookRepository _books;

        public BooksController(ILogger<HomeController> logger,
            ILoanRepository loans,
            IBookRepository books)
        {
            _logger = logger;
            _loans = loans;
            _books = books;
        }
        // GET: BookController
        [Authorize(Roles = "Admin")]
        public IActionResult Management()
        {
            return View("~/Books/Views/Management.cshtml", new BookCreateViewModel());
        }

        [Authorize(Roles = "Reader")]
        public async Task<IActionResult> History(string? q, string status = "all", int page = 1, int pageSize = 10)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue(ClaimTypes.Email)
                  ?? User.Identity?.Name
                  ?? throw new InvalidOperationException("UserId no disponible");

            LoanStatus? st = status?.ToLower() switch
            {
                "borrowed" => LoanStatus.Borrowed,
                "returned" => LoanStatus.Returned,
                _ => (LoanStatus?)null
            };

            var result = await _loans.GetHistoryByUserAsync(userId, q, st, page, pageSize);

            // Mantener filtros en la vista
            result = new PagedResult<LoanHistoryItem>
            {
                Items = result.Items,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                Query = q,
                StatusFilter = status
            };

            return View("~/Books/Views/History.cshtml", result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["OpenCreateModal"] = true; // reabrir modal con errores
                return View("~/Books/Views/Management.cshtml", model);
            }

            // TODO: persistir con tu servicio/repositorio
            await _books.CreateAsync(
                title: model.Title,
                author: model.Author,
                genre: model.Genre,
                availableCopies: model.AvailableCopies
            );

            TempData["ok"] = "Libro creado correctamente.";
            return RedirectToAction(nameof(Management));
        }
    }
}
