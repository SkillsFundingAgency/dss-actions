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
using NCS.DSS.Action.PostActionHttpTrigger.Service;
using NCS.DSS.Action.Validation;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Action.PostActionHttpTrigger.Function
{
    public class PostActionHttpTrigger
    {

        private readonly IResourceHelper _resourceHelper;
        private readonly IPostActionHttpTriggerService _actionsPostService;
        private readonly ILoggerHelper _loggerHelper;
        private readonly IValidate _validate;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IJsonHelper _jsonHelper;
        private readonly IGuidHelper _guidHelper;

        public PostActionHttpTrigger(IResourceHelper resourceHelper, IPostActionHttpTriggerService actionsPostService, ILoggerHelper loggerHelper, IValidate validate, IHttpRequestHelper httpRequestHelper, IHttpResponseMessageHelper httpResponseMessageHelper, IJsonHelper jsonHelper, IGuidHelper guidHelper)
        {
            _resourceHelper = resourceHelper;
            _actionsPostService = actionsPostService;
            _loggerHelper = loggerHelper;
            _validate = validate;
            _httpRequestHelper = httpRequestHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _jsonHelper = jsonHelper;
            _guidHelper = guidHelper;
        }

        [FunctionName("Post")]
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
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionPlanId}/Actions/")]
            HttpRequest req, ILogger log, string customerId, string interactionId, string actionPlanId)
        {

            _loggerHelper.LogMethodEnter(log);

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
                string.Format($"Post Actions C# HTTP trigger function  processed a request. By Touchpoint {touchpointId} and subcontractorId {subcontractorId}"));

            var apimUrl = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimUrl))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'apimurl' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

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

            Models.Action actionRequest;

            try
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to get resource from body of the request");
                actionRequest = await _httpRequestHelper.GetResourceFromRequest<Models.Action>(req);
            }
            catch (JsonException ex)
            {
                _loggerHelper.LogError(log, correlationGuid, "Unable to retrieve body from req", ex);
                return _httpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (actionRequest == null)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Action request is null");
                return _httpResponseMessageHelper.UnprocessableEntity(req);
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to set id's for action");
            actionRequest.SetIds(customerGuid, actionPlanGuid, touchpointId);

            _loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to validate resource");
            var errors = _validate.ValidateResource(actionRequest, true);

            if (errors != null && errors.Any())
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "validation errors with resource");
                return _httpResponseMessageHelper.UnprocessableEntity(errors);
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if customer exists {0}", customerGuid));
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer does not exist {0}", customerGuid));
                return _httpResponseMessageHelper.NoContent(customerGuid);
            }

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if this is a read only customer {0}", customerGuid));
            var isCustomerReadOnly =  _resourceHelper.IsCustomerReadOnly();

            if (isCustomerReadOnly)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer is read only {0}", customerGuid));
                return _httpResponseMessageHelper.Forbidden(customerGuid);
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

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get Create Action Plan for customer {0}", customerGuid));
            var action = await _actionsPostService.CreateAsync(actionRequest);

            if (action != null)
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("attempting to send to service bus {0}", action.ActionId));
                await _actionsPostService.SendToServiceBusQueueAsync(action, apimUrl);
            }

            _loggerHelper.LogMethodExit(log);

            return action == null
                ? _httpResponseMessageHelper.BadRequest(customerGuid)
                : _httpResponseMessageHelper.Created(_jsonHelper.SerializeObjectAndRenameIdProperty(action, "id", "ActionId"));

        }
    }
}