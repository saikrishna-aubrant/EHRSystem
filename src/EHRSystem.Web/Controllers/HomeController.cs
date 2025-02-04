using Microsoft.AspNetCore.Mvc;

namespace EHRSystem.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}