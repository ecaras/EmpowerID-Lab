using Azure.ResourceManager.DataFactory.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;

namespace Empower.Products.ETL.ModelView
{
    public class AdfModelView
    {

        public string ProductName { get; set; } = "";

        public List<string> Pipelines { get; set; } = new List<string>();

        public List<DataFactoryPipelineRunInfo> DataFactoryPipelineRunInfos { get; set; } = new List<DataFactoryPipelineRunInfo>();



    }
}
