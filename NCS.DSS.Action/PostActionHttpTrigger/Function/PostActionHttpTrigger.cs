using DFC.Common.Standard.GuidHelper;
using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Action.Cosmos.Helper;
using NCS.DSS.Action.Models;
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
        private readonly ILogger<PostActionHttpTrigger> _loggerHelper;
        private readonly IValidate _validate;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IConvertToDynamic _convertToDynamic;
        private readonly IGuidHelper _guidHelper;

        public PostActionHttpTrigger(IResourceHelper resourceHelper, IPostActionHttpTriggerService actionsPostService, ILogger<PostActionHttpTrigger> loggerHelper, IValidate validate, IHttpRequestHelper httpRequestHelper, IConvertToDynamic convertToDynamic, IGuidHelper guidHelper)
        {
            _resourceHelper = resourceHelper;
            _actionsPostService = actionsPostService;
            _loggerHelper = loggerHelper;
            _validate = validate;
            _httpRequestHelper = httpRequestHelper;
            _convertToDynamic = convertToDynamic;
            _guidHelper = guidHelper;
        }

        [Function("Post")]
        [ProducesResponseType(typeof(Models.Action), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Action Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Action does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Action validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new Action for a customer. <br>" +
                                              "<br> <b>Validation Rules:</b> <br>" +
                                              "<br><b>DateActionAgreed:</b> DateActionAgreed >= DateTime.Now <br>" +
                                              "<br><b>DateActionAimsToBeCompletedBy:</b> DateActionAimsToBeCompletedBy >= DateActionAgreed <br>" +
                                              "<br><b>DateActionActuallyCompleted:</b> DateActionActuallyCompleted >= DateActionAgreed <br>")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionPlanId}/Actions/")]
            HttpRequest req, string customerId, string interactionId, string actionPlanId)
        {

            _loggerHelper.LogInformation("Start PostActionHttpTrigger");

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

            _loggerHelper.LogInformation(
                string.Format("Post Actions C# HTTP trigger function  processed a request. By Touchpoint: {0}",
                    touchpointId));

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

            Models.Action actionRequest;

            try
            {
                _loggerHelper.LogInformation($"[{correlationId}] Attempt to get resource from body of the request");
                actionRequest = await _httpRequestHelper.GetResourceFromRequest<Models.Action>(req);
            }
            catch (Exception ex)
            {
                _loggerHelper.LogError($"[{correlationId}] Unable to retrieve body from req", ex);
                return new UnprocessableEntityObjectResult(_convertToDynamic.ExcludeProperty(ex, ["TargetSite", "InnerException"]));
            }

            if (actionRequest == null)
            {
                _loggerHelper.LogWarning($"[{correlationId}] Action request is null");
                return new UnprocessableEntityObjectResult(req);
            }

            _loggerHelper.LogInformation($"[{correlationId}] Attempt to set id's for action");
            actionRequest.SetIds(customerGuid, actionPlanGuid, touchpointId);

            _loggerHelper.LogInformation($"[{correlationId}] Attempt to validate resource");
            var errors = _validate.ValidateResource(actionRequest, true);

            if (errors != null && errors.Any())
            {
                _loggerHelper.LogWarning($"[{correlationId}] validation errors with resource");
                return new UnprocessableEntityObjectResult(errors);
            }


            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _loggerHelper.LogError($"Customer with [{customerGuid}] does not exist");
                return new NoContentResult();
            }

            _loggerHelper.LogInformation($"[{correlationId}] Attempting to see if this is a read only customer {customerGuid}");
            var isCustomerReadOnly = _resourceHelper.IsCustomerReadOnly();

            if (isCustomerReadOnly)
            {
                _loggerHelper.LogError($"[{correlationId}] Customer is read only {customerGuid}");
                return new ObjectResult(customerGuid) { StatusCode = (int)HttpStatusCode.Forbidden };
            }

            _loggerHelper.LogInformation($"[{correlationId}] Attempting to see if interaction exists {customerGuid}");
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

            _loggerHelper.LogInformation($"[{correlationId}] Attempting to get Create Action Plan for customer {customerGuid}");
            var action = await _actionsPostService.CreateAsync(actionRequest);

            if (action != null)
            {
                _loggerHelper.LogInformation($"[{correlationId}] Attempting to send to service bus {action.ActionId}");
                await _actionsPostService.SendToServiceBusQueueAsync(action, apimUrl);
            }

            _loggerHelper.LogInformation("Exit from PostActionHttpTrigger");

            return action == null
                ? new BadRequestObjectResult(customerGuid)
                : new JsonResult(action, new JsonSerializerOptions()) { StatusCode = (int)HttpStatusCode.Created };

        }
    }
}
