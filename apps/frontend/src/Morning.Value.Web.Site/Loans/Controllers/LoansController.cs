using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Morning.Value.Web.Site.Loans.Controllers
{
    [Authorize(Roles = "Reader")]
    [Route("loans")]
    public class LoansController : Controller
    {
        private readonly LibraryService _svc; // tu servicio de dominio

        public LoansController(LibraryService svc) => _svc = svc;

        [HttpPost("borrow")]                                 // <-- SOLO POST
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrow([FromForm] int bookId, [FromForm] string? returnUrl)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity!.Name!;
            var ok = await _svc.BorrowAsync(userId: int.Parse(userId), bookId: bookId); // ajusta tipos si tu userId no es int
            TempData[ok ? "ok" : "err"] = ok ? "Préstamo registrado." : "No hay disponibilidad.";
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost("return")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return([FromForm] int loanId, [FromForm] string? returnUrl)
        {
            var ok = await _svc.ReturnAsync(loanId); // cambia estado y disponibilidad

            TempData[ok ? "ok" : "err"] = ok
                ? "Libro devuelto correctamente."
                : "No se pudo devolver el libro.";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("History", "Books");
        }
    }
}
