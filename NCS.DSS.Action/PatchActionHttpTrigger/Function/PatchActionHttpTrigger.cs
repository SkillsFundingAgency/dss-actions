using DFC.Common.Standard.GuidHelper;
using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Action.Cosmos.Helper;
using NCS.DSS.Action.Models;
using NCS.DSS.Action.PatchActionHttpTrigger.Service;
using NCS.DSS.Action.Validation;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Action.PatchActionHttpTrigger.Function
{
    public class PatchActionHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IPatchActionHttpTriggerService _actionsPatchService;
        private readonly ILogger<PatchActionHttpTrigger> _loggerHelper;
        private readonly IValidate _validate;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IConvertToDynamic _convertToDynamic;
        private readonly IGuidHelper _guidHelper;

        public PatchActionHttpTrigger(IResourceHelper resourceHelper, IPatchActionHttpTriggerService actionsPatchService, ILogger<PatchActionHttpTrigger> loggerHelper, IValidate validate, IHttpRequestHelper httpRequestHelper, IConvertToDynamic convertToDynamic, IGuidHelper guidHelper)
        {
            _resourceHelper = resourceHelper;
            _actionsPatchService = actionsPatchService;
            _loggerHelper = loggerHelper;
            _validate = validate;
            _httpRequestHelper = httpRequestHelper;
            _convertToDynamic = convertToDynamic;
            _guidHelper = guidHelper;
        }

        [Function("Patch")]
        [ProducesResponseType(typeof(Models.Action), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Action Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Action does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Action validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update a customers Action record <br>" +
                                               "<br> <b>Validation Rules:</b> <br>" +
                                               "<br><b>DateActionAgreed:</b> DateActionAgreed >= DateTime.Now <br>" +
                                               "<br><b>DateActionAimsToBeCompletedBy:</b> DateActionAimsToBeCompletedBy >= DateActionAgreed <br>" +
                                               "<br><b>DateActionActuallyCompleted:</b> DateActionActuallyCompleted >= DateActionAgreed <br>")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionPlanId}/Actions/{actionId}")]
            HttpRequest req, string customerId, string interactionId, string actionPlanId, string actionId)
        {

            _loggerHelper.LogInformation("Start PatchActionHttpTrigger");

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _loggerHelper.LogWarning($"[{correlationId}] Unable to locate 'TouchpointId' in request header");
                return new BadRequestResult();
            }

            var apimUrl = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimUrl))
            {
                _loggerHelper.LogInformation($"[{correlationId}] Unable to locate 'apimurl' in request header");
                return new BadRequestResult();
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _loggerHelper.LogWarning($"[{correlationId}] Invalid 'CustomerId' in request. CustomerId can't be parsed to Guid");
                return new BadRequestObjectResult(customerGuid);
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                _loggerHelper.LogWarning($"[{correlationId}] Invalid 'interactionId' in request. interactionId can't be parsed to Guid");
                return new BadRequestObjectResult(interactionGuid);
            }

            if (!Guid.TryParse(actionPlanId, out var actionPlanGuid))
            {
                _loggerHelper.LogWarning($"[{correlationId}] Invalid 'actionPlanId' in request. actionPlanId can't be parsed to Guid");
                return new BadRequestObjectResult(actionPlanGuid);
            }

            if (!Guid.TryParse(actionId, out var actionGuid))
            {
                _loggerHelper.LogWarning($"[{correlationId}] Invalid 'actionId' in request. actionId can't be parsed to Guid");
                return new BadRequestObjectResult(actionId);
            }

            ActionPatch actionPatchRequest;

            try
            {
                _loggerHelper.LogInformation("Attempt to get resource from body of the request");
                actionPatchRequest = await _httpRequestHelper.GetResourceFromRequest<ActionPatch>(req);
            }
            catch (Exception ex)
            {
                _loggerHelper.LogError("Unable to retrieve body from req", ex);
                return new UnprocessableEntityObjectResult(_convertToDynamic.ExcludeProperty(ex, ["TargetSite", "InnerException"]));
            }

            if (actionPatchRequest == null)
            {
                _loggerHelper.LogError("Action patch request is null");
                return new UnprocessableEntityObjectResult(req);
            }

            _loggerHelper.LogInformation("Attempt to set id's for action patch");
            actionPatchRequest.SetIds(touchpointId);

            _loggerHelper.LogInformation($"[{correlationId}] Attempting to see if customer exists {customerGuid}");
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _loggerHelper.LogError($"[{correlationId}] Customer does not exist {customerGuid}");
                return new NoContentResult();
            }

            _loggerHelper.LogError($"[{correlationId}] Attempting to see if this is a read only customer {customerGuid}");
            var isCustomerReadOnly = _resourceHelper.IsCustomerReadOnly();

            if (isCustomerReadOnly)
            {
                _loggerHelper.LogError($"[{correlationId}] Customer is read only {customerGuid}");
                return new ObjectResult(customerGuid) { StatusCode = (int)HttpStatusCode.Forbidden };
            }

            _loggerHelper.LogError($"[{correlationId}] Attempting to see if interaction exists {interactionGuid}");
            var doesInteractionExist = _resourceHelper.DoesInteractionExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                _loggerHelper.LogError($"[{correlationId}] Interaction does not exist {interactionGuid}");
                return new NoContentResult();
            }

            var doesActionPlanExistAndBelongToCustomer = _resourceHelper.DoesActionPlanExistAndBelongToCustomer(actionPlanGuid, interactionGuid, customerGuid);

            if (!doesActionPlanExistAndBelongToCustomer)
            {
                _loggerHelper.LogError($"[{correlationId}] Action Plan does not exist {actionPlanGuid}");
                return new NoContentResult();
            }

            _loggerHelper.LogInformation($"[{correlationId}] Attempting to get action {actionGuid} for customer {customerGuid}");
            var actionForCustomer = await _actionsPatchService.GetActionsForCustomerAsync(customerGuid, actionGuid, actionPlanGuid);

            if (actionForCustomer == null)
            {
                _loggerHelper.LogInformation($"[{correlationId}] Action does not exist {actionGuid}");
                return new NoContentResult();
            }

            var patchedAction = _actionsPatchService.PatchResource(actionForCustomer, actionPatchRequest);

            if (patchedAction == null)
            {
                _loggerHelper.LogInformation($"[{correlationId}] Action does not exist {actionGuid}");
                return new NoContentResult();
            }

            Models.Action actionValidationObject;

            try
            {
                actionValidationObject = JsonSerializer.Deserialize<Models.Action>(patchedAction);
            }
            catch (Exception ex)
            {
                _loggerHelper.LogError("Unable to retrieve body from req", _convertToDynamic.ExcludeProperty(ex, ["TargetSite", "InnerException"]));
                throw;
            }

            if (actionValidationObject == null)
            {
                _loggerHelper.LogError("Action Validation Object is null");
                return new UnprocessableEntityObjectResult(req);
            }

            _loggerHelper.LogInformation("Attempt to validate resource");
            var errors = _validate.ValidateResource(actionValidationObject, false);

            if (errors != null && errors.Any())
            {
                _loggerHelper.LogError("validation errors with resource");
                return new UnprocessableEntityObjectResult(errors);
            }


            _loggerHelper.LogInformation($"[{correlationId}] Attempting to update action {actionGuid}");
            var updatedAction = await _actionsPatchService.UpdateCosmosAsync(patchedAction, actionGuid);

            if (updatedAction != null)
            {
                _loggerHelper.LogInformation($"[{correlationId}] attempting to send to service bus {actionGuid}");
                await _actionsPatchService.SendToServiceBusQueueAsync(updatedAction, customerGuid, apimUrl);
            }

            _loggerHelper.LogInformation("Exit from PatchActionHttpTrigger");

            return updatedAction == null ?
                 new BadRequestObjectResult(actionGuid) :
                new JsonResult(updatedAction, new JsonSerializerOptions()) { StatusCode = (int)HttpStatusCode.OK };
        }
    }
}