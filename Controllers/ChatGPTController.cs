using Empower.Products.ETL.Business;
using Empower.Products.ETL.ModelView;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Empower.Products.ETL.Controllers
{
    public class ChatGPTController : Controller
    {


        /// <summary>
        /// starting page
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            var gpt = new Gpt();
            var gptModelView = new GptModelView();
            gptModelView.GptKey = Environment.GetEnvironmentVariable("GPT_KEY") ?? "";
            gptModelView.AssistantID = Environment.GetEnvironmentVariable("GPT_ASSISTANT_ID") ?? "";
            ViewBag.ThreadID = await gpt.CreateAssistantThread(); ;
            return View(gptModelView);
        }
      
        /// <summary>
        /// Send prompt to GPT assistant
        /// </summary>
        /// <param name="prompt"></param>
        /// <returns></returns>
        public async Task<JsonResult> SendPrompt(string prompt, string? threadID)
        {
            var gpt = new Gpt();
            int delay = 5000;
            var messages = new List<string>();
            if (string.IsNullOrEmpty(threadID))
            {
                threadID = await gpt.CreateAssistantThread();
            }
            ViewBag.ThreadID = threadID;
            var messageID = await gpt.CreateThreadMessage(threadID, prompt);
            (var runID, var status) = await gpt.GptRunThread(threadID);
            while (status != "completed" && delay <= 60000)
            {
                delay = delay + 1000;
                status = await gpt.GetThreadStatus(threadID, runID);    //get run status
                if (status != "completed")
                    await Task.Delay(delay);
            }
            if (status.Equals("completed"))
            {
                messages = await gpt.GetThreadMessages(threadID, runID);
            }
            return Json(messages);
        }

    }
}
