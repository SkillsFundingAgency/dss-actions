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
using NCS.DSS.Action.Models;
using NCS.DSS.Action.PatchActionHttpTrigger.Service;
using NCS.DSS.Action.Validation;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Action.PatchActionHttpTrigger.Function
{
    public class PatchActionHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IPatchActionHttpTriggerService _actionsPatchService;
        private readonly ILoggerHelper _loggerHelper;
        private readonly IValidate _validate;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IJsonHelper _jsonHelper;
        private readonly IGuidHelper _guidHelper;

        public PatchActionHttpTrigger(IResourceHelper resourceHelper, IPatchActionHttpTriggerService actionsPatchService, ILoggerHelper loggerHelper, IValidate validate, IHttpRequestHelper httpRequestHelper, IHttpResponseMessageHelper httpResponseMessageHelper, IJsonHelper jsonHelper, IGuidHelper guidHelper)
        {
            _resourceHelper = resourceHelper;
            _actionsPatchService = actionsPatchService;
            _loggerHelper = loggerHelper;
            _validate = validate;
            _httpRequestHelper = httpRequestHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _jsonHelper = jsonHelper;
            _guidHelper = guidHelper;
        }

        [FunctionName("Patch")]
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
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionPlanId}/Actions/{actionId}")]
            HttpRequest req, ILogger log, string customerId, string interactionId, string actionPlanId, string actionId)
        {

            _loggerHelper.LogMethodEnter(log);

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'TouchpointId' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            var apimUrl = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimUrl))
            {
                log.LogInformation("Unable to locate 'apimurl' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
                return _httpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return _httpResponseMessageHelper.BadRequest(interactionGuid);

            if (!Guid.TryParse(actionPlanId, out var actionPlanGuid))
                return _httpResponseMessageHelper.BadRequest(actionPlanGuid);

            if (!Guid.TryParse(actionId, out var actionGuid))
                return _httpResponseMessageHelper.BadRequest(actionGuid);

            ActionPatch actionPatchRequest;

            try
            {
                log.LogInformation("Attempt to get resource from body of the request");
                actionPatchRequest = await _httpRequestHelper.GetResourceFromRequest<ActionPatch>(req);
            }
            catch (JsonException ex)
            {
                log.LogError("Unable to retrieve body from req", ex);
                return _httpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (actionPatchRequest == null)
            {
                log.LogError("Action patch request is null");
                return _httpResponseMessageHelper.UnprocessableEntity(req);
            }

            log.LogInformation("Attempt to set id's for action patch");
            actionPatchRequest.SetIds(touchpointId);

            log.LogInformation(string.Format("Attempting to see if customer exists {0}", customerGuid));
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                log.LogError(string.Format("Customer does not exist {0}", customerGuid));
                return _httpResponseMessageHelper.NoContent(customerGuid);
            }

            log.LogError(string.Format("Attempting to see if this is a read only customer {0}", customerGuid));
            var isCustomerReadOnly = _resourceHelper.IsCustomerReadOnly();

            if (isCustomerReadOnly)
            {
                log.LogError(string.Format("Customer is read only {0}", customerGuid));
                return _httpResponseMessageHelper.Forbidden(customerGuid);
            }

            log.LogError(string.Format("Attempting to see if interaction exists {0}", interactionGuid));
            var doesInteractionExist = _resourceHelper.DoesInteractionExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                log.LogError(string.Format("Interaction does not exist {0}", interactionGuid));
                return _httpResponseMessageHelper.NoContent(interactionGuid);
            }

            var doesActionPlanExistAndBelongToCustomer = _resourceHelper.DoesActionPlanExistAndBelongToCustomer(actionPlanGuid, interactionGuid, customerGuid);

            if (!doesActionPlanExistAndBelongToCustomer)
            {
                log.LogError(string.Format("Action Plan does not exist {0}", actionPlanGuid));
                return _httpResponseMessageHelper.NoContent(actionPlanGuid);
            }

            log.LogInformation(string.Format("Attempting to get action {0} for customer {1}", actionGuid, customerGuid));
            var actionForCustomer = await _actionsPatchService.GetActionsForCustomerAsync(customerGuid, actionGuid, actionPlanGuid);

            if (actionForCustomer == null)
            {
                log.LogInformation(string.Format("Action does not exist {0}", actionGuid));
                return _httpResponseMessageHelper.NoContent(actionGuid);
            }

            var patchedAction = _actionsPatchService.PatchResource(actionForCustomer, actionPatchRequest);

            if (patchedAction == null)
            {
                log.LogInformation(string.Format("Action does not exist {0}", actionGuid));
                return _httpResponseMessageHelper.NoContent(actionGuid);
            }

            Models.Action actionValidationObject;

            try
            {
                actionValidationObject = JsonConvert.DeserializeObject<Models.Action>(patchedAction);
            }
            catch (JsonException ex)
            {
                log.LogError("Unable to retrieve body from req", ex);
                throw;
            }

            if (actionValidationObject == null)
            {
                log.LogError("Action Validation Object is null");
                return _httpResponseMessageHelper.UnprocessableEntity(req);
            }

            log.LogInformation("Attempt to validate resource");
            var errors = _validate.ValidateResource(actionValidationObject, false);

            if (errors != null && errors.Any())
            {
                log.LogError("validation errors with resource");
                return _httpResponseMessageHelper.UnprocessableEntity(errors);
            }


            log.LogError(string.Format("Attempting to update action {0}", actionGuid));
            var updatedAction = await _actionsPatchService.UpdateCosmosAsync(patchedAction, actionGuid);

            if (updatedAction != null)
            {
                log.LogError(string.Format("attempting to send to service bus {0}", actionGuid));
                await _actionsPatchService.SendToServiceBusQueueAsync(updatedAction, customerGuid, apimUrl);
            }

            _loggerHelper.LogMethodExit(log);

            return updatedAction == null ?
                _httpResponseMessageHelper.BadRequest(actionGuid) :
                _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectAndRenameIdProperty(updatedAction, "id", "ActionId"));
        }
    }
}