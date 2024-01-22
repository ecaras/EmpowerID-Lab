using Microsoft.AspNetCore.Mvc;

namespace Empower.Products.ETL.Controllers
{
    public class SQLController : Controller
    {

        /// <summary>
        /// SQL starting page
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Performance tuning
        /// </summary>
        /// <returns></returns>
        public IActionResult PerformanceTuning()
        {
            return View();
        }


        /// <summary>
        /// Change data capture
        /// </summary>
        /// <returns></returns>
        public IActionResult ChangeDataCapture()
        {
            return View(); 
        }

    }
}
