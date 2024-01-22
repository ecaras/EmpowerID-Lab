using Microsoft.AspNetCore.Mvc;

namespace Empower.Products.ETL.Controllers
{
    public class SearchController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
