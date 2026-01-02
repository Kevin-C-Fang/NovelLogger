using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NovelLogger.Controllers
{
    [Authorize]
    public class LibraryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
