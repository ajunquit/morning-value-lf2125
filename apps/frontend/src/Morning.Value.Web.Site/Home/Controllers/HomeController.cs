using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morning.Value.Application.Books.Services;
using Morning.Value.Web.Site.Home.Models;
using Morning.Value.Web.Site.Models;
using System.Diagnostics;

namespace Morning.Value.Web.Site.Home.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBookAppService _bookAppService;

        public HomeController(ILogger<HomeController> logger,
            IBookAppService bookAppService)
        {
            _logger = logger;
            _bookAppService = bookAppService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
                return View("~/Home/Views/Index.Admin.cshtml"); 

            if (User.IsInRole("Reader")) {
                var data = await _bookAppService.GetAllAsync();
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
                return View("~/Home/Views/Index.Reader.cshtml", vm);
            }

            return View("~/Home/Views/Index.Generic.cshtml");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
