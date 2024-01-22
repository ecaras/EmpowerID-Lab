using Microsoft.AspNetCore.Mvc;

namespace Empower.Products.ETL.Controllers
{
    public class CognitiveController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
