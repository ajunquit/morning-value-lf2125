using Microsoft.AspNetCore.Mvc;

namespace Morning.Value.Web.Site.Auth.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult SignIn()
        {
            return View("~/Auth/Views/SignIn.cshtml");
        }

        public IActionResult SignUp()
        {
            return View("~/Auth/Views/SignUp.cshtml");
        }
    }
}
