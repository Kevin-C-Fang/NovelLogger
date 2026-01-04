using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovelLogger.Models;

namespace NovelLogger.Controllers
{
    [Authorize]
    public class LibraryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }
    }
}
