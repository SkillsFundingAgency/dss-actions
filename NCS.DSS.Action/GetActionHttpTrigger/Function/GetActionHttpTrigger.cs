using DFC.Common.Standard.GuidHelper;
using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Action.Cosmos.Helper;
using NCS.DSS.Action.GetActionHttpTrigger.Service;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;


namespace NCS.DSS.Action.GetActionHttpTrigger.Function
{
    public class GetActionHttpTrigger
    {

        private readonly IResourceHelper _resourceHelper;
        private readonly IGetActionHttpTriggerService _actionsGetService;
        private readonly ILogger<GetActionHttpTrigger> _loggerHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IGuidHelper _guidHelper;

        public GetActionHttpTrigger(IResourceHelper resourceHelper, IGetActionHttpTriggerService actionsGetService, ILogger<GetActionHttpTrigger> loggerHelper, IHttpRequestHelper httpRequestHelper, IGuidHelper guidHelper)
        {
            _resourceHelper = resourceHelper;
            _actionsGetService = actionsGetService;
            _loggerHelper = loggerHelper;
            _httpRequestHelper = httpRequestHelper;
            _guidHelper = guidHelper;
        }

        [Function("Get")]
        [ProducesResponseType(typeof(Models.Action), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Action found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Action does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to return all Action for the given Interactions.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionPlanId}/Actions/")]
            HttpRequest req, string customerId, string interactionId, string actionPlanId)
        {

            _loggerHelper.LogInformation("Start GetActionHttpTrigger");
            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _loggerHelper.LogWarning($"[{correlationId}] Unable to locate 'TouchpointId' in request header");
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

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);
            if (!doesCustomerExist)
            {
                _loggerHelper.LogWarning($"[{correlationId}] Customer with [{customerGuid}] does not exist");
                return new NoContentResult();
            }

            var doesInteractionExist = _resourceHelper.DoesInteractionExistAndBelongToCustomer(interactionGuid, customerGuid);
            if (!doesInteractionExist)
            {
                _loggerHelper.LogWarning($"[{correlationId}] Interaction with [{interactionGuid}] does not exist");
                return new NoContentResult();
            }

            var doesActionPlanExistAndBelongToCustomer = _resourceHelper.DoesActionPlanExistAndBelongToCustomer(actionPlanGuid, interactionGuid, customerGuid);
            if (!doesActionPlanExistAndBelongToCustomer)
            {
                _loggerHelper.LogWarning($"[{correlationId}] Action Plan with [{actionPlanGuid}] does not exist");
                return new NoContentResult();
            }

            var actionPlans = await _actionsGetService.GetActionsAsync(customerGuid, actionPlanGuid);

            _loggerHelper.LogInformation("Exit from GetActionHttpTrigger");

            return actionPlans == null ?
                 new NoContentResult() :
                new JsonResult(actionPlans) { StatusCode = (int)HttpStatusCode.OK };
        }
    }
}