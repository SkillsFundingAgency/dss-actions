using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.Action.Annotations;
using Newtonsoft.Json;

namespace NCS.DSS.Action.PatchActionHttpTrigger
{
    public static class PatchActionHttpTrigger
    {
        [FunctionName("Patch")]
        [ResponseType(typeof(Models.Action))]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Action Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Action does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Action validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to update an existing action record.")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionPlanId}/Actions/{actionId}")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId, string actionPlanId, string actionId)
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