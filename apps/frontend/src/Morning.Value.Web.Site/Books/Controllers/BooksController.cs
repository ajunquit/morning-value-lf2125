using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morning.Value.Web.Site.Books.Models;
using Morning.Value.Web.Site.Common.Models;
using Morning.Value.Web.Site.Home.Controllers;
using Morning.Value.Web.Site.Loans;
using Morning.Value.Web.Site.Loans.Enums;
using Morning.Value.Web.Site.Loans.Models;
using System.Security.Claims;

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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Management(string? q, int page = 1, int pageSize = 10)
        {
            var grid = await _books.SearchAsync(q, page, pageSize);

            var vm = new BookManagementViewModel
            {
                Create = new BookCreateViewModel(),
                Grid = grid,
                Query = q
            };

            return View("~/Books/Views/Management.cshtml", vm);
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
                ViewData["OpenCreateModal"] = true;

                // Volver a armar la grilla con los filtros actuales de la URL
                var q = Request.Query["q"].ToString();
                var page = int.TryParse(Request.Query["page"], out var p) ? p : 1;
                var pageSize = int.TryParse(Request.Query["pageSize"], out var ps) ? ps : 10;

                var grid = await _books.SearchAsync(q, page, pageSize);

                var vm = new BookManagementViewModel
                {
                    Create = model,
                    Grid = grid,
                    Query = q
                };

                return View("~/Books/Views/Management.cshtml", vm);
            }

            await _books.CreateAsync(
                title: model.Title,
                author: model.Author,
                genre: model.Genre,
                availableCopies: model.AvailableCopies
            );

            TempData["ok"] = "Libro creado correctamente.";
            // Preservar filtros actuales
            return RedirectToAction(nameof(Management), new { q = Request.Query["q"], page = Request.Query["page"], pageSize = Request.Query["pageSize"] });
        }
    }
}
