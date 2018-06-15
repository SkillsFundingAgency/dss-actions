using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.Action.Annotations;

namespace NCS.DSS.Action.PostActionHttpTrigger
{
    public static class PostActionHttpTrigger
    {
        [FunctionName("Post")]
        [ResponseType(typeof(Models.Action))]
        [ActionResponse(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Action Created", ShowSchema = true)]
        [ActionResponse(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Unable to create Action", ShowSchema = false)]
        [ActionResponse(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Forbidden", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new action for a given action plan.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionPlanId}/Actions/")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId, string actionPlanId)
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