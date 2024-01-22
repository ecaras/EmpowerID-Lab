using Empower.Products.ETL.Business;
using Empower.Products.ETL.ModelView;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;
using System.Text.Json;

namespace Empower.Products.ETL.Controllers
{
    public class AdfController : Controller
    {

        /// <summary>
        /// Index page for ETL monitoring
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            var adf = new Adf();
            var adfModelView = new AdfModelView();
            var pipelines = await adf.GetPipelineNames();
            adfModelView.Pipelines = pipelines.OrderBy(o => o).ToList();      
            //adfModelView.DataFactoryPipelineRunInfos = await adf.GetAdfLogs();  ////get logs after window load
            return View(adfModelView);
        }

        /// <summary>
        /// For ajax run pipeline
        /// </summary>
        /// <param name="pipelineName">Pipeline name to run</param>
        /// <returns></returns>
        public async Task<string> RunPipeline(string pipelineName)
        {
            var adf = new Adf();
            //var runID = "adasaf";
            var runID = await adf.RunPipeline(pipelineName);
            return runID;
        }

        /// <summary>
        /// Get ADF logs
        /// </summary>
        /// <returns>returns partialview logs</returns>
        [HttpGet]
        public async Task<ActionResult> RefreshLogs()
        {
            var adf = new Adf();
            var adfModelView = new AdfModelView();
            adfModelView.DataFactoryPipelineRunInfos = await adf.GetAdfLogs();
            return PartialView("~/Views/Adf/_AdfLogs.cshtml", adfModelView);
        }

        /// <summary>
        /// Unused
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index2()
        {
            var adf = new Adf();
            var adfModelView = new AdfModelView();
            var pipelines = await adf.GetPipelineNames();
            adfModelView.Pipelines = pipelines;
            adfModelView.DataFactoryPipelineRunInfos = await adf.GetAdfLogs();
            return View(adfModelView);
        }
    }
}
