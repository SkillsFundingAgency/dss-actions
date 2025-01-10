using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Action.Cosmos.Helper;
using NCS.DSS.Action.GetActionByIdHttpTrigger.Service;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Action.GetActionByIdHttpTrigger.Function
{
    public class GetActionByIdHttpTrigger
    {
        private readonly IGetActionByIdHttpTriggerService _actionGetByIdService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly ILogger<GetActionByIdHttpTrigger> _logger;

        public GetActionByIdHttpTrigger(
            IGetActionByIdHttpTriggerService actionGetByIdService, 
            IHttpRequestHelper httpRequestHelper,
            IResourceHelper resourceHelper, 
            ILogger<GetActionByIdHttpTrigger> logger)
        {
            _actionGetByIdService = actionGetByIdService;
            _httpRequestHelper = httpRequestHelper;
            _resourceHelper = resourceHelper;
            _logger = logger;
        }

        [Function("GETBYID")]
        [ProducesResponseType(typeof(Models.Action), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Action found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Action does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "GET_BY_ACTIONID", Description = "Ability to retrieve an individual Action for the given customer")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionPlanId}/Actions/{actionId}")]
            HttpRequest req, string customerId, string interactionId, string actionPlanId, string actionId)
        {
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(GetActionByIdHttpTrigger));

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

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);
            if (!doesCustomerExist)
            {
                _logger.LogWarning("Customer does not exist. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);

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
            var action = await _actionGetByIdService.GetActionForCustomerAsync(customerGuid, actionGuid, actionPlanGuid);

            if (action == null)
            {
                _logger.LogInformation("Action does not exist for Customer. Action GUID: {ActionGuid}. Customer GUID: {CustomerGuid}", actionGuid, customerGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetActionByIdHttpTrigger));
                return new NoContentResult();
            }

            _logger.LogInformation("Successfully retrieved Action for Customer. Action ID: {ActionId}. Customer GUID: {CustomerGuid}", action.ActionId.GetValueOrDefault(), customerGuid);
            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetActionByIdHttpTrigger));
            return new JsonResult(action, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}
