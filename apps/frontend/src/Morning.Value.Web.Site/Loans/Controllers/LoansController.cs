using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morning.Value.Application.Loans.Services;
using System.Security.Claims;

namespace Morning.Value.Web.Site.Loans.Controllers
{
    [Authorize(Roles = "Reader")]
    [Route("loans")]
    public class LoansController : Controller
    {
        private readonly ILoanAppService _loadAppService;

        public LoansController(ILoanAppService loadAppService) => _loadAppService = loadAppService;

        [HttpPost("borrow")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrow([FromForm] string bookId, [FromForm] string? returnUrl)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity!.Name!;
            var result = await _loadAppService.BorrowAsync(Guid.Parse(userId), Guid.Parse(bookId));
            var ok = result != null;
            TempData[ok ? "ok" : "err"] = ok ? "Préstamo registrado." : "No hay disponibilidad.";
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost("return")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return([FromForm] Guid loanId, [FromForm] string? returnUrl)
        {
            var ok = await _loadAppService.ReturnAsync(loanId);

            TempData[ok ? "ok" : "err"] = ok
                ? "Libro devuelto correctamente."
                : "No se pudo devolver el libro.";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("History", "Books");
        }
    }
}
