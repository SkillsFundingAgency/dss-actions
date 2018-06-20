using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Action.Annotations;

namespace NCS.DSS.Action.GetActionHttpTrigger
{
    public static class GetActionHttpTrigger
    {
        [FunctionName("Get")]
        [ActionResponse(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Actions found", ShowSchema = true)]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionPlanId}/Actions/")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId, string actionPlanId)
        {
            log.Info("Get Actions C# HTTP trigger function processed a request.");

            var service = new GetActionHttpTriggerService();
            var values = await service.GetActions();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(values),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}