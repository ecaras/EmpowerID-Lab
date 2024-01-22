using Empower.Products.ETL.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Empower.Products.ETL.Business;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Humanizer;

namespace Empower.Products.ETL.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Initialized
        /// </summary>
        /// <param name="logger"></param>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Home starting page
        /// </summary>
        /// <param name="logger"></param>
        public IActionResult Index()
        {
            _logger.LogInformation("Starting the application.");
            return View();
        }

        /// <summary>
        /// Test call
        /// </summary>
        public async void Test()
        {
            var adf = new Adf();
            var gpt = new Gpt();
            int delay = 5000;
            var threadID = await gpt.CreateAssistantThread();
            var messageID = await gpt.CreateThreadMessage(threadID, "what is 1 + 1?");
            (var runID, var status) = await gpt.GptRunThread(threadID);
            while (status != "completed" && delay <= 120000)
            {
                //delay = delay + 5000;
                status = await gpt.GetThreadStatus(threadID, runID);    //get run status            
                await Task.Delay(delay);
            }
            if (status.Equals("completed"))
            {
                var messages = await gpt.GetThreadMessages(threadID, runID);
            }
            var t = "";
        }

        /// <summary>
        /// Privacy statement *unused
        /// </summary>
        /// <returns></returns>
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}