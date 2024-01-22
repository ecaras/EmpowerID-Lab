using Empower.Products.ETL.Business;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace Empower.Products.ETL.Controllers
{
    public class LogController : Controller
    {

        private readonly ILogger<LogController> _logger;


        public LogController(ILogger<LogController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Log starting page
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        public void ThrowApplicationLog(string prompt)
        {
            try {
                int result = Int32.Parse("test");   //should throw an error
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message.ToString());
            }            
        }
    }
}
