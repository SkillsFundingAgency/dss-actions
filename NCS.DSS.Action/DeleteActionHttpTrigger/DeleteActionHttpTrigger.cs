using System;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace NCS.DSS.Action.DeleteActionHttpTrigger
{
    public static class DeleteActionHttpTrigger
    {
        [FunctionName("Delete")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "Customers/{customerId:guid}/Interactions/{interactionId:guid}/ActionPlans/{actionplanId:guid}/Actions/{actionId:guid}")]HttpRequestMessage req, TraceWriter log, string actionId)
        {
            log.Info("Delete Action C# HTTP trigger function processed a request.");

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
                Content = new StringContent("Deleted Action record with Id of : " + actionGuid)
            };
        }
    }
}