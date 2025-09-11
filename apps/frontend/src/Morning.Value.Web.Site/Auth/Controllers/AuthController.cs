using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morning.Value.Application.Users.Services;
using Morning.Value.Domain.Users.Enums;
using Morning.Value.Web.Site.Auth.Models;
using System.Security.Claims;

namespace Morning.Value.Web.Site.Auth.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthAppService _authAppService;

        public AuthController(IAuthAppService authAppService) => _authAppService = authAppService;

        [HttpGet, AllowAnonymous]
        public IActionResult SignIn(string? returnUrl = null)
        {
            // si ya está autenticado, NO muestres el login
            if (User.Identity?.IsAuthenticated ?? false)
            {
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View("~/Auth/Views/SignIn.cshtml", new SignInViewModel());
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignInViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authAppService.SignInAsync(model.Email!, model.Password!);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "No se pudo iniciar sesión.");
                return View("~/Auth/Views/SignIn.cshtml", model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.UserId!.Value.ToString()),
                new Claim(ClaimTypes.Name, result.Name ?? string.Empty),
                new Claim(ClaimTypes.Email, result.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, (result.Role ?? RoleType.Reader).ToString())
            };

            // 2) Construir principal + propiedades (persistente si RememberMe)
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var props = new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            // 3) Firmar (crea cookie de auth)
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

            // 4) Redirigir a returnUrl local o al Home (Books/Index si quieres)
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return (result.Role == RoleType.Admin)
                 ? RedirectToAction("Management", "Books")
                 : RedirectToAction("History", "Books");
        }

        [HttpGet, AllowAnonymous]
        public IActionResult SignUp()
            => View("~/Auth/Views/SignUp.cshtml", new SignUpViewModel());

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Auth/Views/SignUp.cshtml", model);

            var r = await _authAppService.SignUpAsync(model.Name!, model.Email!, model.Password!, RoleType.Reader);
            if (!r.Success)
            {
                ModelState.AddModelError(string.Empty, r.Error ?? "No se pudo registrar el usuario.");
                return View("~/Auth/Views/SignUp.cshtml", model);
            }

            TempData["ok"] = "Usuario registrado. Inicia sesión.";
            return RedirectToAction(nameof(SignIn));
        }

        [HttpPost]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(SignIn));
        }

        [HttpGet, AllowAnonymous]
        public IActionResult Denied() => Content("Acceso denegado");
    }
}
