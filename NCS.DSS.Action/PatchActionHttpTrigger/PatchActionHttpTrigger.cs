using System;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace NCS.DSS.Action.PatchActionHttpTrigger
{
    public static class PatchActionHttpTrigger
    {
        [FunctionName("Patch")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId:guid}/Interactions/{interactionId:guid}/ActionPlans/{actionplanId:guid}/Actions/{actionId:guid}")]HttpRequestMessage req, TraceWriter log, string actionId)
        {
            log.Info("Patch Action C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(actionId, out var actionGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(actionId),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Updated Action record with Id of : " + actionGuid)
            };
        }
    }
}