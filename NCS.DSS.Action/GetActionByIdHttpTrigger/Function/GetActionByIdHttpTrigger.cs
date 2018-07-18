using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Action.Annotations;
using NCS.DSS.Action.Cosmos.Helper;
using NCS.DSS.Action.GetActionByIdHttpTrigger.Service;
using NCS.DSS.Action.Helpers;
using NCS.DSS.Action.Ioc;

namespace NCS.DSS.Action.GetActionByIdHttpTrigger.Function
{
    public static class GetActionByIdHttpTrigger
    {
        [FunctionName("GetById")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Actions found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Actions do not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionPlanId}/Actions/{actionId}")]HttpRequestMessage req, ILogger log, string customerId, string interactionId, string actionPlanId, string actionId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IGetActionByIdHttpTriggerService actionGetService)
        {
            log.LogInformation("Get Action By Id C# HTTP trigger function  processed a request.");

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return HttpResponseMessageHelper.BadRequest(interactionGuid);

            if (!Guid.TryParse(actionPlanId, out var actionPlanGuid))
                return HttpResponseMessageHelper.BadRequest(actionPlanGuid);

            if (!Guid.TryParse(actionId, out var actionGuid))
                return HttpResponseMessageHelper.BadRequest(actionGuid);

            var doesCustomerExist = resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var doesInteractionExist = resourceHelper.DoesInteractionExist(interactionGuid);

            if (!doesInteractionExist)
                return HttpResponseMessageHelper.NoContent(interactionGuid);

            var doesActionPlanExist = resourceHelper.DoesActionPlanExist(actionPlanGuid);

            if (!doesActionPlanExist)
                return HttpResponseMessageHelper.NoContent(actionPlanGuid);

            var action = await actionGetService.GetActionForCustomerAsync(customerGuid, actionGuid);

            return action == null ?
                HttpResponseMessageHelper.NoContent(customerGuid) :
                HttpResponseMessageHelper.Ok(action);

        }
    }
}