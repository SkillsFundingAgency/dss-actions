using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Action.Annotations;
using NCS.DSS.Action.Cosmos.Helper;
using NCS.DSS.Action.Helpers;
using NCS.DSS.Action.Ioc;
using NCS.DSS.Action.PatchActionHttpTrigger.Service;
using NCS.DSS.Action.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.Action.PatchActionHttpTrigger.Function
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
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionPlanId}/Actions/{actionId}")]HttpRequestMessage req, ILogger log, string customerId, string interactionId, string actionPlanId, string actionId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IHttpRequestMessageHelper httpRequestMessageHelper,
            [Inject]IValidate validate,
            [Inject]IPatchActionHttpTriggerService actionPatchService)
        {
            var touchpointId = httpRequestMessageHelper.GetTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'TouchpointId' in request header");
                return HttpResponseMessageHelper.BadRequest();
            }

            var ApimURL = httpRequestMessageHelper.GetApimURL(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                log.LogInformation("Unable to locate 'apimurl' in request header");
                return HttpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("Patch Action HTTP trigger function processed a request. By Touchpoint " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return HttpResponseMessageHelper.BadRequest(interactionGuid);

            if (!Guid.TryParse(actionPlanId, out var actionPlanGuid))
                return HttpResponseMessageHelper.BadRequest(actionPlanGuid);

            if (!Guid.TryParse(actionId, out var actionGuid))
                return HttpResponseMessageHelper.BadRequest(actionGuid);

            Models.ActionPatch actionPatchRequest;

            try
            {
                actionPatchRequest = await httpRequestMessageHelper.GetActionFromRequest<Models.ActionPatch>(req);
            }
            catch (JsonException ex)
            {
                return HttpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (actionPatchRequest == null)
                return HttpResponseMessageHelper.UnprocessableEntity(req);

            actionPatchRequest.LastModifiedTouchpointId = touchpointId;

            var errors = validate.ValidateResource(actionPatchRequest, false);

            if (errors != null && errors.Any())
                return HttpResponseMessageHelper.UnprocessableEntity(errors);

            var doesCustomerExist = await resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var isCustomerReadOnly = await resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
                return HttpResponseMessageHelper.Forbidden(customerGuid);

            var doesInteractionExistAndBelongToCustomer = resourceHelper.DoesInteractionExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExistAndBelongToCustomer)
                return HttpResponseMessageHelper.NoContent(interactionGuid);

            var doesActionPlanExistAndBelongToCustomer = resourceHelper.DoesActionPlanExistAndBelongToCustomer(actionPlanGuid, interactionGuid, customerGuid);

            if (!doesActionPlanExistAndBelongToCustomer)
                return HttpResponseMessageHelper.NoContent(actionPlanGuid);

            var actionForCustomer = await actionPatchService.GetActionForCustomerAsync(customerGuid, actionGuid);

            if (actionForCustomer == null)
                return HttpResponseMessageHelper.NoContent(actionGuid);

            var patchedAction = actionPatchService.PatchResource(actionForCustomer, actionPatchRequest);

            if (patchedAction == null)
                return HttpResponseMessageHelper.NoContent(actionGuid);
            
            var updatedAction = await actionPatchService.UpdateCosmosAsync(patchedAction, actionGuid);

            if (updatedAction != null)
                await actionPatchService.SendToServiceBusQueueAsync(updatedAction, customerGuid, ApimURL);

            return updatedAction == null ?
                HttpResponseMessageHelper.BadRequest(actionPlanGuid) :
                HttpResponseMessageHelper.Ok(JsonHelper.SerializeObject(updatedAction));
        }
    }
}