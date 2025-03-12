using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Action.Cosmos.Helper;
using NCS.DSS.Action.PostActionHttpTrigger.Service;
using NCS.DSS.Action.Validation;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Action.PostActionHttpTrigger.Function
{
    public class PostActionHttpTrigger
    {

        private readonly IResourceHelper _resourceHelper;
        private readonly IPostActionHttpTriggerService _actionsPostService;
        private readonly ILogger<PostActionHttpTrigger> _logger;
        private readonly IValidate _validate;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IDynamicHelper _dynamicHelper;

        private static readonly string[] PropertiesToExclude = { "TargetSite", "InnerException" };

        public PostActionHttpTrigger(
            IPostActionHttpTriggerService actionsPostService, 
            IHttpRequestHelper httpRequestHelper, 
            IResourceHelper resourceHelper, 
            IValidate validate, 
            IDynamicHelper dynamicHelper,
            ILogger<PostActionHttpTrigger> logger)
        {
            _actionsPostService = actionsPostService;
            _httpRequestHelper = httpRequestHelper;
            _resourceHelper = resourceHelper;
            _validate = validate;
            _dynamicHelper = dynamicHelper;
            _logger = logger;
        }

        [Function("POST")]
        [ProducesResponseType(typeof(Models.Action), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Action Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Action does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.UnprocessableEntity, Description = "Action validation error(s)", ShowSchema = false)]
        [Display(Name = "POST", Description = "Ability to create a new Action for a customer. <br>" +
                                              "<br> <b>Validation Rules:</b> <br>" +
                                              "<br><b>DateActionAgreed:</b> DateActionAgreed >= DateTime.Now <br>" +
                                              "<br><b>DateActionAimsToBeCompletedBy:</b> DateActionAimsToBeCompletedBy >= DateActionAgreed <br>" +
                                              "<br><b>DateActionActuallyCompleted:</b> DateActionActuallyCompleted >= DateActionAgreed <br>")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionPlanId}/Actions/")]
            HttpRequest req, string customerId, string interactionId, string actionPlanId)
        {
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(PostActionHttpTrigger));

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogInformation("Unable to locate 'DssCorrelationId' in request header");
            }

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                _logger.LogInformation("Unable to parse 'DssCorrelationId' to a GUID");
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

            _logger.LogInformation("Header validation has succeeded. Touchpoint ID: {TouchpointId}. Correlation GUID: {CorrelationGuid}", touchpointId, correlationGuid);

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

            Models.Action actionRequest;

            try
            {
                _logger.LogInformation("Attempting to retrieve resource from request body. Correlation GUID: {CorrelationGuid}", correlationGuid);
                actionRequest = await _httpRequestHelper.GetResourceFromRequest<Models.Action>(req);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to parse {ActionRequest} from request body. Correlation GUID: {CorrelationGuid}. Exception: {Exception}", nameof(actionRequest), correlationGuid, ex.Message);
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, PropertiesToExclude));
            }

            if (actionRequest == null)
            {
                _logger.LogError("{ActionRequest} object is NULL. Correlation GUID: {CorrelationGuid}", nameof(actionRequest), correlationGuid);
                return new UnprocessableEntityObjectResult(req);
            }

            _logger.LogInformation("Initialise IDs for Action");
            actionRequest.SetIds(customerGuid, actionPlanGuid, touchpointId);

            _logger.LogInformation("Attempting to validate {ActionRequest} object", nameof(actionRequest));
            var errors = _validate.ValidateResource(actionRequest, true);

            if (errors != null && errors.Any())
            {
                _logger.LogError("Failed to validate {ActionRequest}. Correlation GUID: {CorrelationGuid}", nameof(actionRequest), correlationGuid);
                return new UnprocessableEntityObjectResult(errors);
            }

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);
            if (!doesCustomerExist)
            {
                _logger.LogWarning("Customer does not exist. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);

            _logger.LogInformation("Attempting to check if Customer is read only. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
            {
                _logger.LogWarning("Customer is read-only. Operation is forbidden. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
                return new ObjectResult(customerGuid)
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
            }
            _logger.LogInformation("Customer is not read-only. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);

            _logger.LogInformation("Attempting to check if Interaction exists. Interaction GUID: {InteractionGuid}. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", interactionGuid, customerGuid, correlationGuid);
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

            _logger.LogInformation("Attempting to create Action for Customer. Action ID: {ActionId}. Customer GUID: {CustomerGuid}", actionRequest.ActionId, customerGuid);
            var action = await _actionsPostService.CreateAsync(actionRequest);

            if (action == null)
            {
                _logger.LogWarning("PATCH request unsuccessful. Customer GUID: {CustomerGuid}", customerGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PostActionHttpTrigger));
                return new BadRequestObjectResult(customerGuid);
            }

            _logger.LogInformation("Attempting to send Action to service bus. Action ID: {ActionId}. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", action.ActionId.GetValueOrDefault(), customerGuid, correlationGuid);
            await _actionsPostService.SendToServiceBusQueueAsync(action, apimUrl);
            
            _logger.LogInformation("POST request successful. Action ID: {ActionId}", action.ActionId.GetValueOrDefault());
            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PostActionHttpTrigger));

            return new JsonResult(action, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.Created
            };
        }
    }
}
