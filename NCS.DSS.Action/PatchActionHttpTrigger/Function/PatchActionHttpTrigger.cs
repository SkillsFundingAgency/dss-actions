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
        private readonly IPatchActionHttpTriggerService _actionsPatchService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly IValidate _validate;
        private readonly IDynamicHelper _dynamicHelper;
        private readonly ILogger<PatchActionHttpTrigger> _logger;

        private static readonly string[] PropertiesToExclude = { "TargetSite", "InnerException" };

        public PatchActionHttpTrigger(
            IPatchActionHttpTriggerService actionsPatchService, 
            IHttpRequestHelper httpRequestHelper, 
            IResourceHelper resourceHelper, 
            IValidate validate, 
            IDynamicHelper dynamicHelper,
            ILogger<PatchActionHttpTrigger> logger)
        {
            _actionsPatchService = actionsPatchService;
            _httpRequestHelper = httpRequestHelper;
            _resourceHelper = resourceHelper;
            _validate = validate;
            _dynamicHelper = dynamicHelper;
            _logger = logger;
        }

        [Function("PATCH")]
        [ProducesResponseType(typeof(Models.Action), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Action Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Action does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.UnprocessableEntity, Description = "Action validation error(s)", ShowSchema = false)]
        [Display(Name = "PATCH", Description = "Ability to modify/update a customers Action record <br>" +
                                               "<br> <b>Validation Rules:</b> <br>" +
                                               "<br><b>DateActionAgreed:</b> DateActionAgreed >= DateTime.Now <br>" +
                                               "<br><b>DateActionAimsToBeCompletedBy:</b> DateActionAimsToBeCompletedBy >= DateActionAgreed <br>" +
                                               "<br><b>DateActionActuallyCompleted:</b> DateActionActuallyCompleted >= DateActionAgreed <br>")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionPlanId}/Actions/{actionId}")]
            HttpRequest req, string customerId, string interactionId, string actionPlanId, string actionId)
        {
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(PatchActionHttpTrigger));

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogInformation("Unable to locate 'DssCorrelationId' in request header");
            }

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                _logger.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogWarning("Unable to locate 'TouchpointId' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new BadRequestResult();
            }

            var apimUrl = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimUrl))
            {
                _logger.LogInformation("Unable to locate 'apimUrl' in request header");
                return new BadRequestResult();
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogWarning("Unable to parse 'customerId' to a GUID. Customer ID: {CustomerId}. Correlation GUID: {CorrelationGuid}", customerId, correlationGuid);
                return new BadRequestObjectResult(customerGuid);
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                _logger.LogWarning("Unable to parse 'interactionId' to a GUID. Interaction ID: {InteractionId}. Correlation GUID: {CorrelationGuid}", interactionId, correlationGuid);
                return new BadRequestObjectResult(interactionGuid);
            }

            if (!Guid.TryParse(actionPlanId, out var actionPlanGuid))
            {
                _logger.LogWarning("Unable to parse 'actionPlanId' to a GUID. Action Plan ID: {ActionPlanId}. Correlation GUID: {CorrelationGuid}", actionPlanId, correlationGuid);
                return new BadRequestObjectResult(actionPlanGuid);
            }

            if (!Guid.TryParse(actionId, out var actionGuid))
            {
                _logger.LogWarning("Unable to parse 'actionId' to a GUID. Action ID: {ActionId}. Correlation GUID: {CorrelationGuid}", actionId, correlationGuid);
                return new BadRequestObjectResult(actionGuid);
            }

            _logger.LogInformation("Header validation has succeeded. Touchpoint ID: {TouchpointId}. Correlation GUID: {CorrelationGuid}", touchpointId, correlationGuid);

            ActionPatch actionPatchRequest;

            try
            {
                _logger.LogInformation("Attempting to retrieve resource from request body. Correlation GUID: {CorrelationGuid}", correlationGuid);
                actionPatchRequest = await _httpRequestHelper.GetResourceFromRequest<ActionPatch>(req);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to parse {ActionPatch} from request body. Correlation GUID: {CorrelationGuid}. Exception: {Exception}", nameof(actionPatchRequest), correlationGuid, ex.Message);
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, PropertiesToExclude));
            }
            _logger.LogInformation("Retrieved resource from request body. Correlation GUID: {CorrelationGuid}", correlationGuid);

            if (actionPatchRequest == null)
            {
                _logger.LogError("{ActionPatch} object is NULL. Correlation GUID: {CorrelationGuid}", nameof(actionPatchRequest), correlationGuid);
                return new UnprocessableEntityObjectResult(req);
            }

            _logger.LogInformation("Initialise TouchPoint ID for Action");
            actionPatchRequest.SetIds(touchpointId);

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogWarning("Customer does not exist. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);

            _logger.LogInformation("Attempting to check if customer is read only. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
            {
                _logger.LogWarning("Customer is read-only. Operation is forbidden. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
                return new ObjectResult(customerGuid.ToString())
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
            }
            _logger.LogInformation("Customer is not read-only. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);

            _logger.LogInformation("Attempting to check if interaction exists. Interaction GUID: {InteractionGuid}. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", interactionGuid, customerGuid, correlationGuid);
            var doesInteractionExist = await _resourceHelper.DoesInteractionExistAndBelongToCustomer(interactionGuid, customerGuid);
            if (!doesInteractionExist)
            {
                _logger.LogWarning("Interaction does not exist. Interaction GUID: {InteractionGuid}. Correlation GUID: {CorrelationGuid}", interactionGuid, correlationGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Interaction exists. Interaction GUID: {InteractionGuid}. Correlation GUID: {CorrelationGuid}", interactionGuid, correlationGuid);

            _logger.LogInformation("Attempting to check if Action Plan exists and is assigned to Customer. Interaction GUID: {InteractionGuid}. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", interactionGuid, customerGuid, correlationGuid);
            var doesActionPlanExistAndBelongToCustomer = await _resourceHelper.DoesActionPlanExistAndBelongToCustomer(actionPlanGuid, interactionGuid, customerGuid);
            if (!doesActionPlanExistAndBelongToCustomer)
            {
                _logger.LogWarning("Action Plan does not exist and is not assigned to Customer. Action Plan GUID: {ActionPlanGuid}. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", actionPlanGuid, customerGuid, correlationGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Action Plan exists and is assigned to Customer. Action Plan GUID: {ActionPlanGuid}. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", actionPlanGuid, customerGuid, correlationGuid);

            _logger.LogInformation("Attempting to retrieve Action for Customer. Action GUID: {ActionGuid}. Customer GUID: {CustomerGuid}", actionGuid, customerGuid);
            var actionForCustomer = await _actionsPatchService.GetActionsForCustomerAsync(customerGuid, actionGuid, actionPlanGuid);

            if (actionForCustomer == null)
            {
                _logger.LogInformation("Action does not exist for Customer. Action GUID: {ActionGuid}. Customer GUID: {CustomerGuid}", actionGuid, customerGuid);
                return new NoContentResult();
            }

            _logger.LogInformation("Attempting to update Action for customer. Customer GUID: {CustomerGuid}. Action GUID: {ActionGuid}", customerGuid, actionGuid);
            var patchedAction = _actionsPatchService.PatchResource(actionForCustomer, actionPatchRequest);

            if (patchedAction == null)
            {
                _logger.LogInformation("Failed to update Action for customer. The function {FunctionName} has returned NULL or empty. Customer GUID: {CustomerGuid}. Action GUID: {ActionGuid}", nameof(_actionsPatchService.PatchResource), customerGuid, actionGuid);
                return new NoContentResult();
            }

            Models.Action actionValidationObject;

            try
            {
                _logger.LogInformation("Attempting to deserialize {PatchedAction} validation object. Customer GUID: {CustomerGuid}. Action GUID: {ActionGuid}", nameof(patchedAction), customerGuid, actionGuid);
                actionValidationObject = JsonSerializer.Deserialize<Models.Action>(patchedAction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured when attempting to deserialize {patchedAction} validation object. Customer GUID: {CustomerGuid}. Action GUID: {ActionGuid}. Error message: {ErrorMessage}", nameof(patchedAction), ex.Message, customerGuid, actionGuid);
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, PropertiesToExclude));
            }

            if (actionValidationObject == null)
            {
                _logger.LogWarning("Deserializing {ActionValidationObject} validation object has returned NULL. Customer GUID: {CustomerGuid}. Action GUID: {ActionGuid}", nameof(actionValidationObject), customerGuid, actionGuid);
                return new UnprocessableEntityObjectResult(req);
            }

            _logger.LogInformation("Attempting to validate {ActionValidationObject} object", nameof(actionValidationObject));
            var errors = _validate.ValidateResource(actionValidationObject, false);

            if (errors != null && errors.Any())
            {
                _logger.LogError("Falied to validate {ActionValidationObject}", nameof(actionValidationObject));
                return new UnprocessableEntityObjectResult(errors);
            }

            _logger.LogInformation("Attempting to PATCH an Action. Action GUID: {ActionGuid}. Customer GUID: {CustomerGuid}", actionGuid, customerGuid);
            var updatedAction = await _actionsPatchService.UpdateCosmosAsync(patchedAction, actionGuid);

            if (updatedAction == null)
            {
                _logger.LogWarning("PATCH request unsuccessful. Action GUID: {ActionGuid}", actionGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PatchActionHttpTrigger));

                return new BadRequestObjectResult(actionGuid);
            }

            _logger.LogInformation("Attempting to send Action to service bus. Action ID: {ActionId}. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", updatedAction.ActionId.GetValueOrDefault(), customerGuid, correlationGuid);
            await _actionsPatchService.SendToServiceBusQueueAsync(updatedAction, customerGuid, apimUrl);

            _logger.LogInformation("PATCH request successful. Action ID: {ActionId}", updatedAction.ActionId.GetValueOrDefault());
            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PatchActionHttpTrigger));

            return new JsonResult(updatedAction, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}