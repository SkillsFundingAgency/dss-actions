using System;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace NCS.DSS.Action.PutActionHttpTrigger
{
    public static class PutActionHttpTrigger
    {
        [Disable]
        [FunctionName("Put")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionPlanId}/Actions/{actionId}")]HttpRequestMessage req, TraceWriter log, string actionId)
        {
            log.Info("Put Action C# HTTP trigger function processed a request.");

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
                Content = new StringContent("Replaced Action record with Id of : " + actionGuid)
            };
        }
    }
}