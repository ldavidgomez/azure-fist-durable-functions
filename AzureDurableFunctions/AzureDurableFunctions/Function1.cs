using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzureDurableFunctions
{
    public static class Function1
    {
        [FunctionName("ImportMrp_Orchestator")]
        public static async Task<List<List<string>>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputsGetFromExcel = new List<string>();
            var outputsValidateFiles = new List<string>();
            var outputsSendResults = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputsGetFromExcel.Add(await context.CallActivityAsync<string>("GetMrpMappingsFromExcel", "ExcelFile"));
            Thread.Sleep(3000);
            outputsValidateFiles.Add(await context.CallActivityAsync<string>("ValidateImportMrpItems", "Seattle"));
            Thread.Sleep(3000);
            outputsSendResults.Add(await context.CallActivityAsync<string>("SendImportMrpResult", "London"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return new List<List<string>>() { outputsGetFromExcel, outputsValidateFiles, outputsSendResults };
        }

        [FunctionName("GetMrpMappingsFromExcel")]
        public static string GetMrpMappingsFromExcel([ActivityTrigger] string file, ILogger log)
        {
            log.LogInformation($"Read items from file {file}.");
            return $"Items readed {file}!";
        }

        [FunctionName("ValidateImportMrpItems")]
        public static string ValidateImportMrpItems([ActivityTrigger] string file, ILogger log)
        {
            log.LogInformation($"Validating items from file {file}.");
            return $"Items validated {file}!";
        }

        [FunctionName("SendImportMrpResult")]
        public static string SendImportMrpResult([ActivityTrigger] string file, ILogger log)
        {
            log.LogInformation($"Read items from file {file}.");
            return $"Send import results {file}!";
        }

        [FunctionName("ImportMrp")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("ImportMrp_Orchestator", null);

            log.LogInformation($"Started ImportMrp_Orchestator with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}