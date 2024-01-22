namespace Empower.Products.ETL.Business
{

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Core;
    using Azure.Core.Expressions.DataFactory;
    using Azure.Identity;
    using Azure.ResourceManager;
    using Azure.ResourceManager.DataFactory;
    using Azure.ResourceManager.DataFactory.Models;
    using Azure.ResourceManager.Resources;
    using Empower.Products.ETL.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NuGet.Common;
    using NuGet.Protocol;

    public class Adf
    {

        private readonly IConfiguration _configuration;
        private readonly string _tenantID;
        private readonly string _subscriptionID; 
        private readonly string _clientID; 
        private readonly string _clientSecret;
        private readonly string _resourceGroupName;
        private readonly string _dataFactoryName;
        private readonly string _grantType = "client_credentials";
        private readonly string _resource = "https://management.azure.com/";

        public Adf()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile(Global.AppSettingFile).Build();
            _tenantID = Environment.GetEnvironmentVariable("AZURE_TENANT_ID") ?? ""; 
            _subscriptionID = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID") ?? ""; 
            _clientID = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") ?? "";
            _clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET") ?? "";
            _resourceGroupName = Environment.GetEnvironmentVariable("RESOURCE_GROUP_NAME") ?? "";
            _dataFactoryName = Environment.GetEnvironmentVariable("DATA_FACTORY_NAME") ?? "";
        }


        /// <summary>
        /// Get access token
        /// Help link at https://learn.microsoft.com/en-us/rest/api/datafactory/v2
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetAccessToken() {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://login.microsoftonline.com/{_tenantID}/oauth2/token");            
            var collection = new List<KeyValuePair<string, string>>();
            collection.Add(new("grant_type", _grantType));
            collection.Add(new("client_id", _clientID));
            collection.Add(new("client_secret", _clientSecret));
            collection.Add(new("resource", _resource));
            var content = new FormUrlEncodedContent(collection);
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            //await response.Content.ReadAsStringAsync());
            var responseStr = await response.Content.ReadAsStringAsync();
            dynamic resp = JObject.Parse(responseStr.ToString());
            string access_token = resp.access_token;
            return access_token;
        }


        /// <summary>
        /// Will run the pipeline
        /// </summary>
        /// <param name="pipelineName">Name of data factory pipeline</param>
        /// <returns></returns>
        public async Task<string> RunPipeline(string pipelineName="") {
            var client = new HttpClient();
            var accessToken = await GetAccessToken();
            pipelineName = pipelineName ?? "test1001";
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://management.azure.com/subscriptions/{_subscriptionID}/resourceGroups/{_resourceGroupName}/providers/Microsoft.DataFactory/factories/{_dataFactoryName}/pipelines/{pipelineName}/createRun?api-version=2018-06-01");            
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
            var content = new StringContent(string.Empty);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseStr = await response.Content.ReadAsStringAsync();
            dynamic resp = JObject.Parse(responseStr.ToString());
            string pipelineRunID = resp.runId;
            return pipelineRunID;
        }

        /// <summary>
        /// Wil return list of ADF pipeline name
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetPipelineNames()
        { 
            var pipelines = new List<string>();
            DefaultAzureCredentialOptions options = new DefaultAzureCredentialOptions();
            options.TenantId = _tenantID;                                                   //force tenantid when facing multi-subscription err InvalidAuthenticationTokenTenant 
            options.ManagedIdentityClientId = _clientID;                                    //app registration clientid
            options.DisableInstanceDiscovery = false;
            TokenCredential cred = new DefaultAzureCredential(options);
            ArmClient client = new ArmClient(cred);

            ResourceIdentifier dataFactoryResourceId = DataFactoryResource.CreateResourceIdentifier(_subscriptionID, _resourceGroupName, _dataFactoryName);
            DataFactoryResource dataFactory = client.GetDataFactoryResource(dataFactoryResourceId);

            // get the collection of this DataFactoryPipelineResource
            DataFactoryPipelineCollection collection = dataFactory.GetDataFactoryPipelines();
            //var tt = await collection.GetAllAsync();
            // invoke the operation and iterate over the result
            try
            {
                await foreach (var item in collection.GetAllAsync())
                {
                    // the variable item is a resource, you could call other operations on this instance as well
                    var asd = item;
                    DataFactoryPipelineData resourceData = item.Data;
                    pipelines.Add(resourceData.Name);
                }
            }
            catch(InvalidOperationException ex){
                //expected error at the end of list
                var s = ex;
            }
            return pipelines;
        }

        /// <summary>
        /// Get ADF pipelines trigger run logs
        /// </summary>
        /// <remarks></remarks>
        public async Task<List<DataFactoryPipelineRunInfo>> GetAdfLogs()
        {           
            DefaultAzureCredentialOptions options = new DefaultAzureCredentialOptions();
            options.TenantId = _tenantID;                                                   //force tenantid when facing multi-subscription err InvalidAuthenticationTokenTenant 
            options.ManagedIdentityClientId = _clientID;                                    //app registration clientid
            options.DisableInstanceDiscovery = false;                                       
            TokenCredential cred = new DefaultAzureCredential(options);
            // authenticate your client
            ArmClient client = new ArmClient(cred, _subscriptionID);//with subscription id
            ResourceIdentifier dataFactoryResourceId = DataFactoryResource.CreateResourceIdentifier(_subscriptionID, _resourceGroupName, _dataFactoryName);
            DataFactoryResource dataFactory = client.GetDataFactoryResource(dataFactoryResourceId);
            //add filter
            RunFilterContent content = new RunFilterContent(DateTimeOffset.Parse(DateTime.Now.AddDays(-3).ToString()), DateTimeOffset.Parse(DateTime.Now.AddDays(1).ToString()))
            {
                Filters =
                {
                    new RunQueryFilter(RunQueryFilterOperand.PipelineName,RunQueryFilterOperator.NotEquals,new string[]
                    {
                        "yourcustomfilter"
                    })
                },
            };
            var pipelineRunInfos = new List<DataFactoryPipelineRunInfo>();
            //invoke the operation and iterate over the result
            await foreach (DataFactoryPipelineRunInfo item in dataFactory.GetPipelineRunsAsync(content))
            {
                pipelineRunInfos.Add(item);
            }
            return pipelineRunInfos.OrderByDescending(a => a.RunStartOn).ToList();
        }




    }
}
