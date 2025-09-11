using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morning.Value.Web.Site.Auth.Models;
using System.Security.Claims;

namespace Morning.Value.Web.Site.Auth.Controllers
{
    public class AuthController : Controller
    {
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

            // TODO Parte 2: validar credenciales con tu servicio (simulado OK aquí)
            var isValid = true; // <- reemplazar
            if (!isValid)
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas");
                return View(model);
            }

            // 1) Claims del usuario
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "123"),        // id de tu usuario
                new Claim(ClaimTypes.Name, "Alejo"),                // nombre
                new Claim(ClaimTypes.Email, model.Email)            // email
                // puedes añadir roles/otros claims si quieres
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
            return RedirectToAction("Index", "Books"); // o donde quieras entrar
        }

        [HttpGet, AllowAnonymous]
        public IActionResult SignUp()
            => View("~/Auth/Views/SignUp.cshtml", new SignUpViewModel());

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public IActionResult SignUp(SignUpViewModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Auth/Views/SignUp.cshtml", model);

            // TODO: Registrar usuario (Parte 2). Por ahora, simular éxito:
            // var ok = UserService.Create(model.Name, model.Email, model.Password);
            // if(!ok) { ModelState.AddModelError("", "El correo ya está registrado."); return View(...); }

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
