using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace NCS.DSS.Action.PostActionHttpTrigger
{
    public static class PostActionHttpTrigger
    {
        [FunctionName("Post")]
        [ResponseType(typeof(Models.Action))]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId:guid}/Interactions/{interactionId:guid}/ActionPlans/{actionplanId:guid}/Actions/")]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("Post Action C# HTTP trigger function processed a request.");

            // Get request body
            var action = await req.Content.ReadAsAsync<Models.Action>();

            var actionService = new PostActionHttpTriggerService();
            var actionId = actionService.Create(action);

            return actionId == null
                ? new HttpResponseMessage(HttpStatusCode.BadRequest)
                : new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent("Created Action record with Id of : " + actionId)
                };
        }
    }
}