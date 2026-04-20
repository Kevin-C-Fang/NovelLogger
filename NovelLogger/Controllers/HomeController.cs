using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NovelLogger.Models;
using NovelLogger.Models.ViewModels;
using System.Diagnostics;

namespace NovelLogger.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Bookmark");
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (feature?.Error != null)
            {
                _logger.LogError(feature.Error, "Unhandled exception at {Path} with TraceId {TraceId}.", feature.Path, HttpContext.TraceIdentifier);
            }

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
