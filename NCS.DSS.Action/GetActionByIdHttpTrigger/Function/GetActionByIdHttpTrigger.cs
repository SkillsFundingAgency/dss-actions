using DFC.Common.Standard.GuidHelper;
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

namespace NCS.DSS.Action.GetActionByIdHttpTrigger.Function
{
    public class GetActionByIdHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IGetActionByIdHttpTriggerService _actionGetByIdService;
        private readonly ILogger<GetActionByIdHttpTrigger> _loggerHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IGuidHelper _guidHelper;

        public GetActionByIdHttpTrigger(IResourceHelper resourceHelper, IGetActionByIdHttpTriggerService actionGetByIdService, ILogger<GetActionByIdHttpTrigger> loggerHelper, IHttpRequestHelper httpRequestHelper, IGuidHelper guidHelper)
        {
            _resourceHelper = resourceHelper;
            _actionGetByIdService = actionGetByIdService;
            _loggerHelper = loggerHelper;
            _httpRequestHelper = httpRequestHelper;
            _guidHelper = guidHelper;
        }

        [Function("GetById")]
        [ProducesResponseType(typeof(Models.Action), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Action found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Action does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve an individual Action for the given customer")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionPlanId}/Actions/{actionId}")]
            HttpRequest req, string customerId, string interactionId, string actionPlanId, string actionId)
        {
            _loggerHelper.LogInformation("Start GetActionByIdHttpTrigger");

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

            if (!Guid.TryParse(actionId, out var actionGuid))
            {
                _loggerHelper.LogWarning($"[{correlationId}] Invalid 'actionId' in request. actionId can't be parsed to Guid");
                return new BadRequestObjectResult(actionGuid);
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

            var action = await _actionGetByIdService.GetActionForCustomerAsync(customerGuid, actionGuid, actionPlanGuid);

            _loggerHelper.LogInformation("Exit from GetActionByIdHttpTrigger");

            return action == null ?
                new NoContentResult() :
                new JsonResult(action) { StatusCode = (int)HttpStatusCode.OK };

        }
    }
}
