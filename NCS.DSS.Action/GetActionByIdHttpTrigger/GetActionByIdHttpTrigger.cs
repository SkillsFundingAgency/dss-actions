using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.Action.GetActionByIdHttpTrigger
{
    public static class GetActionByIdHttpTrigger
    {
        [FunctionName("GetById")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId:guid}/Interactions/{interactionId:guid}/ActionPlans/{actionplanId:guid}/Actions/{actionId:guid}")]HttpRequestMessage req, TraceWriter log, string actionId)
        {
            log.Info("Get Action By Id C# HTTP trigger function  processed a request.");

            if (!Guid.TryParse(actionId, out var actionGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(actionId),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            var service = new GetActionByIdHttpTriggerService();
            var values = await service.GetAction(actionGuid);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(values),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}