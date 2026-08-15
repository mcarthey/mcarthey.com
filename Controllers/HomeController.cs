using Microsoft.AspNetCore.Mvc;

namespace mcarthey.com.Controllers
{
    // NoStore + NoCache on the home page so browsers always re-fetch after
    // a deploy. HTML is tiny; the cost is negligible, and it prevents the
    // "phone still holds yesterday's version" surprise.
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
