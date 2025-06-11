using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Action.Cosmos.Helper;
using NCS.DSS.Action.GetActionHttpTrigger.Service;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Action.GetActionHttpTrigger.Function
{
    public class GetActionHttpTrigger
    {
        private readonly IGetActionHttpTriggerService _actionsGetService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly ILogger<GetActionHttpTrigger> _logger;

        public GetActionHttpTrigger(
            IGetActionHttpTriggerService actionsGetService,
            IHttpRequestHelper httpRequestHelper,
            IResourceHelper resourceHelper,
            ILogger<GetActionHttpTrigger> logger)
        {
            _actionsGetService = actionsGetService;
            _httpRequestHelper = httpRequestHelper;
            _resourceHelper = resourceHelper;
            _logger = logger;
        }

        [Function("GET")]
        [ProducesResponseType(typeof(Models.Action), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Action found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Action does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "GET", Description = "Ability to return all Action for the given Interactions.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionPlanId}/Actions/")]
            HttpRequest req, string customerId, string interactionId, string actionPlanId)
        {
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(GetActionHttpTrigger));

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

            _logger.LogInformation("Attempting to retrieve Actions for Customer. Customer GUID: {CustomerGuid}", customerGuid);
            var actions = await _actionsGetService.GetActionsAsync(customerGuid, actionPlanGuid);

            if (actions == null)
            {
                _logger.LogInformation("No Action exist for Customer. Customer GUID: {CustomerGuid}", customerGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetActionHttpTrigger));
                return new NoContentResult();
            }

            if (actions.Count == 1)
            {
                _logger.LogInformation("Action successfully retrieved. Action ID: {ActionId}", actions.FirstOrDefault()!.ActionId);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetActionHttpTrigger));
                return new JsonResult(actions[0], new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
            }

            var actionsIds = actions.Select(a => a.ActionId).ToList();

            _logger.LogInformation("{Count} Actions successfully retrieved. Action IDs: {ActionIds}", actions.Count, actionsIds);
            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetActionHttpTrigger));
            return new JsonResult(actions, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}