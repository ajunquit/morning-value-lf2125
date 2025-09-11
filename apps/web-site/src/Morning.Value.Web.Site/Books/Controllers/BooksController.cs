using Microsoft.AspNetCore.Mvc;
using Morning.Value.Web.Site.Controllers;

namespace Morning.Value.Web.Site.Books.Controllers
{
    public class BooksController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public BooksController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        // GET: BookController
        public IActionResult Index()
        {
            return View("~/Books/Views/Index.cshtml");
        }

        // GET: BookController/Details/5
        public IActionResult Details(int id)
        {
            return View();
        }

        // GET: BookController/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BookController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: BookController/Edit/5
        public IActionResult Edit(int id)
        {
            return View();
        }

        // POST: BookController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: BookController/Delete/5
        public IActionResult Delete(int id)
        {
            return View();
        }

        // POST: BookController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
