using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
