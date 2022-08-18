using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers
{
    public class RespondentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
