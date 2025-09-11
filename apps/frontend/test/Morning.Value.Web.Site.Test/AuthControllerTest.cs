using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Morning.Value.Application.Users.Dtos;
using Morning.Value.Application.Users.Services;
using Morning.Value.Domain.Users.Enums;
using Morning.Value.Web.Site.Auth.Controllers;
using Morning.Value.Web.Site.Auth.Models;
using System.Security.Claims;

namespace Morning.Value.Web.Site.Test
{
    public class AuthController_Tests
    {
        private static AuthController CreateController(
            Mock<IAuthAppService> authSvcMock,
            out Mock<IAuthenticationService> authMock,
            out DefaultHttpContext http,
            ClaimsPrincipal? user = null,
            IUrlHelper? urlHelper = null)
        {
            authMock = new Mock<IAuthenticationService>();

            var services = new ServiceCollection();
            services.AddSingleton<IAuthenticationService>(authMock.Object);
            var sp = services.BuildServiceProvider();

            http = new DefaultHttpContext
            {
                RequestServices = sp,
                User = user ?? new ClaimsPrincipal(new ClaimsIdentity()) // no autenticado por defecto
            };

            var controller = new AuthController(authSvcMock.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = http },
                TempData = new TempDataDictionary(http, new Mock<ITempDataProvider>().Object),
                Url = urlHelper ?? Mock.Of<IUrlHelper>()
            };

            return controller;
        }

        private static ClaimsPrincipal AuthenticatedPrincipal(string? role = null)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, "Tester") };
            if (!string.IsNullOrWhiteSpace(role))
                claims.Add(new Claim(ClaimTypes.Role, role));
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(identity);
        }

        // ---------------- GET /auth/signin ----------------

        [Fact]
        public void Get_SignIn_NotAuthenticated_Returns_View_With_Model_And_ReturnUrl()
        {
            var authSvc = new Mock<IAuthAppService>();
            var controller = CreateController(authSvc, out _, out var http);

            var result = controller.SignIn("/back");

            var view = result as ViewResult;
            view.Should().NotBeNull();
            view!.ViewName.Should().Be("~/Auth/Views/SignIn.cshtml");
            view.Model.Should().BeOfType<SignInViewModel>();
            controller.ViewData["ReturnUrl"].Should().Be("/back");
        }

        [Fact]
        public void Get_SignIn_Authenticated_WithLocalReturnUrl_Redirects_To_ReturnUrl()
        {
            var authSvc = new Mock<IAuthAppService>();
            var url = new Mock<IUrlHelper>();
            url.Setup(u => u.IsLocalUrl("/home")).Returns(true);

            var user = AuthenticatedPrincipal(RoleType.Admin.ToString());
            var controller = CreateController(authSvc, out _, out var http, user, url.Object);

            var result = controller.SignIn("/home");

            var redir = result as RedirectResult;
            redir.Should().NotBeNull();
            redir!.Url.Should().Be("/home");
        }

        [Fact]
        public void Get_SignIn_Authenticated_NoReturnUrl_Redirects_HomeIndex()
        {
            var authSvc = new Mock<IAuthAppService>();
            var user = AuthenticatedPrincipal();
            var controller = CreateController(authSvc, out _, out var http, user);

            var result = controller.SignIn(null);

            var redir = result as RedirectToActionResult;
            redir.Should().NotBeNull();
            redir!.ActionName.Should().Be("Index");
            redir.ControllerName.Should().Be("Home");
        }

        // ---------------- POST /auth/signin ----------------

        [Fact]
        public async Task Post_SignIn_InvalidModel_Returns_View_With_Same_Model()
        {
            var authSvc = new Mock<IAuthAppService>();
            var controller = CreateController(authSvc, out _, out var http);
            controller.ModelState.AddModelError("Email", "Requerido");

            var model = new SignInViewModel { Email = "", Password = "" };
            var result = await controller.SignIn(model);

            var view = result as ViewResult;
            view.Should().NotBeNull();
            view!.Model.Should().Be(model);
        }

        [Fact]
        public async Task Post_SignIn_Service_Fails_Returns_View_With_ModelError()
        {
            var authSvc = new Mock<IAuthAppService>();
            authSvc.Setup(s => s.SignInAsync("u@acme.com", "pwd", default))
                   .ReturnsAsync(new AuthResult { Success = false, Error = "bad" });

            var controller = CreateController(authSvc, out _, out var http);
            var model = new SignInViewModel { Email = "u@acme.com", Password = "pwd" };

            var result = await controller.SignIn(model);

            var view = result as ViewResult;
            view.Should().NotBeNull();
            view!.ViewName.Should().Be("~/Auth/Views/SignIn.cshtml");
            controller.ModelState.ErrorCount.Should().Be(1);
        }

        [Fact]
        public async Task Post_SignIn_Success_Admin_Redirects_To_Books_Management_And_SignsIn()
        {
            var authSvc = new Mock<IAuthAppService>();
            var userId = Guid.NewGuid();

            authSvc.Setup(s => s.SignInAsync("admin@acme.com", "pwd", default))
                   .ReturnsAsync(new AuthResult
                   {
                       Success = true,
                       UserId = userId,
                       Name = "Admin",
                       Email = "admin@acme.com",
                       Role = RoleType.Admin
                   });

            var controller = CreateController(authSvc, out var authMock, out var http);
            var model = new SignInViewModel { Email = "admin@acme.com", Password = "pwd" };

            var result = await controller.SignIn(model);

            // Verifica redirección
            var redir = result as RedirectToActionResult;
            redir.Should().NotBeNull();
            redir!.ActionName.Should().Be("Management");
            redir.ControllerName.Should().Be("Books");

            // Verifica que se haya llamado SignInAsync del AuthenticationService
            authMock.Verify(a => a.SignInAsync(
                http, CookieAuthenticationDefaults.AuthenticationScheme,
                It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()),
                Times.Once);
        }

        [Fact]
        public async Task Post_SignIn_Success_Reader_With_Local_ReturnUrl_Redirects_To_ReturnUrl()
        {
            var authSvc = new Mock<IAuthAppService>();
            var userId = Guid.NewGuid();

            authSvc.Setup(s => s.SignInAsync("u@acme.com", "pwd", default))
                   .ReturnsAsync(new AuthResult
                   {
                       Success = true,
                       UserId = userId,
                       Name = "Reader",
                       Email = "u@acme.com",
                       Role = RoleType.Reader
                   });

            var url = new Mock<IUrlHelper>();
            url.Setup(u => u.IsLocalUrl("/volver")).Returns(true);

            var controller = CreateController(authSvc, out var authMock, out var http, null, url.Object);
            var model = new SignInViewModel { Email = "u@acme.com", Password = "pwd" };

            var result = await controller.SignIn(model, "/volver");

            var redir = result as RedirectResult;
            redir.Should().NotBeNull();
            redir!.Url.Should().Be("/volver");

            authMock.Verify(a => a.SignInAsync(
                http, CookieAuthenticationDefaults.AuthenticationScheme,
                It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()),
                Times.Once);
        }

        // ---------------- GET /auth/signup ----------------

        [Fact]
        public void Get_SignUp_Returns_View_With_Model()
        {
            var authSvc = new Mock<IAuthAppService>();
            var controller = CreateController(authSvc, out _, out var http);

            var result = controller.SignUp();

            var view = result as ViewResult;
            view.Should().NotBeNull();
            view!.ViewName.Should().Be("~/Auth/Views/SignUp.cshtml");
            view.Model.Should().BeOfType<SignUpViewModel>();
        }

        // ---------------- POST /auth/signup ----------------

        [Fact]
        public async Task Post_SignUp_InvalidModel_Returns_View_With_Model()
        {
            var authSvc = new Mock<IAuthAppService>();
            var controller = CreateController(authSvc, out _, out var http);
            controller.ModelState.AddModelError("Email", "Requerido");

            var model = new SignUpViewModel { Name = "", Email = "", Password = "", ConfirmPassword = "" };

            var result = await controller.SignUp(model);

            var view = result as ViewResult;
            view.Should().NotBeNull();
            view!.ViewName.Should().Be("~/Auth/Views/SignUp.cshtml");
            view.Model.Should().Be(model);
        }

        [Fact]
        public async Task Post_SignUp_Service_Fails_Returns_View_With_ModelError()
        {
            var authSvc = new Mock<IAuthAppService>();
            authSvc.Setup(s => s.SignUpAsync("Name", "u@acme.com", "pwd", RoleType.Reader, default))
                   .ReturnsAsync(new RegisterResult { Success = false, Error = "exists" });

            var controller = CreateController(authSvc, out _, out var http);
            var model = new SignUpViewModel { Name = "Name", Email = "u@acme.com", Password = "pwd", ConfirmPassword = "pwd" };

            var result = await controller.SignUp(model);

            var view = result as ViewResult;
            view.Should().NotBeNull();
            view!.ViewName.Should().Be("~/Auth/Views/SignUp.cshtml");
            controller.ModelState.ErrorCount.Should().Be(1);
        }

        [Fact]
        public async Task Post_SignUp_Success_Sets_TempData_And_Redirects_To_SignIn()
        {
            var authSvc = new Mock<IAuthAppService>();
            authSvc.Setup(s => s.SignUpAsync("Name", "u@acme.com", "pwd", RoleType.Reader, default))
                   .ReturnsAsync(new RegisterResult { Success = true, UserId = Guid.NewGuid() });

            var controller = CreateController(authSvc, out _, out var http);
            var model = new SignUpViewModel { Name = "Name", Email = "u@acme.com", Password = "pwd", ConfirmPassword = "pwd" };

            var result = await controller.SignUp(model);

            var redir = result as RedirectToActionResult;
            redir.Should().NotBeNull();
            redir!.ActionName.Should().Be(nameof(AuthController.SignIn));

            controller.TempData["ok"].Should().Be("Usuario registrado. Inicia sesion.");
        }

        // ---------------- POST /auth/signout ----------------

        [Fact]
        public async Task Post_SignOut_Calls_Auth_Service_And_Redirects_To_SignIn()
        {
            var authSvc = new Mock<IAuthAppService>();
            var controller = CreateController(authSvc, out var authMock, out var http, AuthenticatedPrincipal());

            var result = await controller.SignOut();

            var redir = result as RedirectToActionResult;
            redir.Should().NotBeNull();
            redir!.ActionName.Should().Be(nameof(AuthController.SignIn));

            authMock.Verify(a => a.SignOutAsync(http, CookieAuthenticationDefaults.AuthenticationScheme, null), Times.Once);
            // Verificamos la extensión de SignOutAsync que usa IAuthenticationService:
            authMock.Verify(a => a.SignOutAsync(http,
                CookieAuthenticationDefaults.AuthenticationScheme,
                It.IsAny<AuthenticationProperties>()),
                Times.Once);
        }

        // ---------------- GET /auth/denied ----------------

        [Fact]
        public void Get_Denied_Returns_Text_Content()
        {
            var authSvc = new Mock<IAuthAppService>();
            var controller = CreateController(authSvc, out _, out var http);

            var result = controller.Denied();

            var content = result as ContentResult;
            content.Should().NotBeNull();
            content!.Content.Should().Be("Acceso denegado");
        }
    }
}