using Microsoft.Build.Framework;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Numerics;
using System.Security.Policy;

namespace Empower.Products.ETL.Business
{


    public class Gpt
    {
        private string _gptKey;
        private string _assistantID;
        private string _gptApiEndpoint = "https://api.openai.com";

        public Gpt()
        {
            _gptKey = Environment.GetEnvironmentVariable("GPT_KEY") ?? "";
            _assistantID = Environment.GetEnvironmentVariable("GPT_ASSISTANT_ID") ?? "";
        }

        /// <summary>
        /// Create a thread for assistant message
        /// </summary>
        /// <returns></returns>
        public async Task<string> CreateAssistantThread() {
            var options = new RestClientOptions(_gptApiEndpoint) {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("/v1/threads", RestSharp.Method.Post);
            request.AddHeader("Authorization", $"Bearer {_gptKey}");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("OpenAI-Beta", "assistants=v1");
            RestResponse response = await client.ExecuteAsync(request);
            dynamic resp = JObject.Parse(response?.Content ?? "");
            var threadID = resp.id;
            return threadID;
        }

        /// <summary>
        /// Create a message for a thread
        /// </summary>
        /// <param name="threadID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<string> CreateThreadMessage(string threadID, string message)
        {
            var options = new RestClientOptions(_gptApiEndpoint) {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest($"/v1/threads/{threadID}/messages", RestSharp.Method.Post);
            request.AddHeader("Authorization", $"Bearer {_gptKey}");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("OpenAI-Beta", "assistants=v1");
            var body = @"{""role"": ""user"",""content"": """ + message + "\"}";
            request.AddStringBody(body, DataFormat.Json);
            RestResponse response = await client.ExecuteAsync(request);
            dynamic resp = JObject.Parse(response?.Content ?? "");
            var messageID = resp.id;
            return messageID;
        }

        /// <summary>
        /// Send the thread to GPT assistant
        /// </summary>
        /// <param name="threadID"></param>
        /// <returns></returns>
        public async Task<(string, string)> GptRunThread(string threadID)
        {
            var options = new RestClientOptions(_gptApiEndpoint) {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest($"/v1/threads/{threadID}/runs", RestSharp.Method.Post);
            request.AddHeader("Authorization", $"Bearer {_gptKey}");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("OpenAI-Beta", "assistants=v1");
            var body = @"{""assistant_id"": """ + _assistantID + @""", ""instructions"": ""Please address the user, it has a premium account.""}";
            request.AddStringBody(body, DataFormat.Json);
            RestResponse response = await client.ExecuteAsync(request);
            dynamic resp = JObject.Parse(response?.Content ?? "");
            var runID = resp.id;
            var status = resp.status;
            return (runID, status);
        }

        /// <summary>
        /// Get the thread messages 
        /// </summary>
        /// <param name="threadID"></param>
        /// <returns></returns>
        public async Task<List<string>> GetThreadMessages(string threadID, string runID)
        {
            var msgs = new List<string>();
            var options = new RestClientOptions(_gptApiEndpoint)
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest($"/v1/threads/{threadID}/messages", RestSharp.Method.Get);
            request.AddHeader("Authorization", $"Bearer {_gptKey}");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("OpenAI-Beta", "assistants=v1");
            RestResponse response = await client.ExecuteAsync(request);
            dynamic resp = JObject.Parse(response?.Content ?? "");
            var data = resp.data;
            foreach (dynamic d in data)
            {
                if (d.role == "assistant" && d.run_id == runID)
                {
                    string msg = d.content[0].text.value;
                    //msgs.Add(msg);
                    msgs.Insert(0, msg);
                }
            }
            return msgs;
        }

        /// <summary>
        /// Get thred message status
        /// </summary>
        /// <param name="threadID"></param>
        /// <param name="runID"></param>
        /// <returns></returns>
        public async Task<string> GetThreadStatus(string threadID, string runID)
        {
            var options = new RestClientOptions(_gptApiEndpoint) {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest($"/v1/threads/{threadID}/runs/{runID}", RestSharp.Method.Get);
            request.AddHeader("Authorization", $"Bearer {_gptKey}");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("OpenAI-Beta", "assistants=v1");
            RestResponse response = await client.ExecuteAsync(request);
            dynamic resp = JObject.Parse(response?.Content ?? "");
            var status = resp.status;
            return status;
        }

    }


}
