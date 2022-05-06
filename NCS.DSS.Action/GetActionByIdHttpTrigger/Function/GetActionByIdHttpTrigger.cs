using DFC.Common.Standard.GuidHelper;
using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Action.Cosmos.Helper;
using NCS.DSS.Action.GetActionByIdHttpTrigger.Service;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Action.GetActionByIdHttpTrigger.Function
{
    public class GetActionByIdHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IGetActionByIdHttpTriggerService _actionGetByIdService;
        private readonly ILoggerHelper _loggerHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IJsonHelper _jsonHelper;
        private readonly IGuidHelper _guidHelper;

        public GetActionByIdHttpTrigger(IResourceHelper resourceHelper, IGetActionByIdHttpTriggerService actionGetByIdService, ILoggerHelper loggerHelper, IHttpRequestHelper httpRequestHelper, IHttpResponseMessageHelper httpResponseMessageHelper, IJsonHelper jsonHelper, IGuidHelper guidHelper)
        {
            _resourceHelper = resourceHelper;
            _actionGetByIdService = actionGetByIdService;
            _loggerHelper = loggerHelper;
            _httpRequestHelper = httpRequestHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _jsonHelper = jsonHelper;
            _guidHelper = guidHelper;
        }

        [FunctionName("GetById")]
        [ProducesResponseType(typeof(Models.Action), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Action found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Action does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve an individual Action for the given customer")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionPlanId}/Actions/{actionId}")]
            HttpRequest req, ILogger log, string customerId, string interactionId, string actionPlanId, string actionId)
        {
            
            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            var correlationGuid = _guidHelper.ValidateGuid(correlationId);

            if (correlationGuid == Guid.Empty)
                correlationGuid = _guidHelper.GenerateGuid();

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'TouchpointId' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'SubcontractorId' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid,
                string.Format("Get Action Plan By Id C# HTTP trigger function  processed a request. By Touchpoint: {0}",
                    touchpointId));

            var customerGuid = _guidHelper.ValidateGuid(customerId);
            if (customerGuid == Guid.Empty)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'customerId' to a Guid: {0}", customerId));
                return _httpResponseMessageHelper.BadRequest(customerId);
            }

            var interactionGuid = _guidHelper.ValidateGuid(interactionId);
            if (interactionGuid == Guid.Empty)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'interactionId' to a Guid: {0}", interactionId));
                return _httpResponseMessageHelper.BadRequest(interactionGuid);
            }

            var actionPlanGuid = _guidHelper.ValidateGuid(actionPlanId);
            if (actionPlanGuid == Guid.Empty)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'actionplanId' to a Guid: {0}", actionPlanGuid));
                return _httpResponseMessageHelper.BadRequest(actionPlanGuid);
            }

            var actionGuid = _guidHelper.ValidateGuid(actionId);
            if (actionGuid == Guid.Empty)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'actionId' to a Guid: {0}", actionGuid));
                return _httpResponseMessageHelper.BadRequest(actionGuid);
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if customer exists {0}", customerGuid));
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer does not exist {0}", customerGuid));
                return _httpResponseMessageHelper.NoContent(customerGuid);
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if interaction exists {0}", interactionGuid));
            var doesInteractionExist = _resourceHelper.DoesInteractionExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Interaction does not exist {0}", interactionGuid));
                return _httpResponseMessageHelper.NoContent(interactionGuid);
            }

            var doesActionPlanExistAndBelongToCustomer = _resourceHelper.DoesActionPlanExistAndBelongToCustomer(actionPlanGuid, interactionGuid, customerGuid);

            if (!doesActionPlanExistAndBelongToCustomer)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Action Plan does not exist {0}", actionPlanGuid));
                return _httpResponseMessageHelper.NoContent(actionPlanGuid);
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get action plan {0} for customer {1}", actionGuid, customerGuid));
            var action = await _actionGetByIdService.GetActionForCustomerAsync(customerGuid, actionGuid, actionPlanGuid);

            _loggerHelper.LogMethodExit(log);

            return action == null ?
                _httpResponseMessageHelper.NoContent(actionGuid) :
                _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectAndRenameIdProperty(action, "id", "ActionId"));

        }
    }
}