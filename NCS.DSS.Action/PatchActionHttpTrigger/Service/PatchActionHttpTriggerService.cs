using NCS.DSS.Action.Cosmos.Provider;
using NCS.DSS.Action.Models;
using NCS.DSS.Action.ServiceBus;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.Action.PatchActionHttpTrigger.Service
{
    public class PatchActionHttpTriggerService : IPatchActionHttpTriggerService
    {
        private readonly IActionPatchService _actionPatchService;
        private readonly IDocumentDBProvider _documentDbProvider;
        private readonly IServiceBusClient _serviceBusClient;

        public PatchActionHttpTriggerService(IActionPatchService actionPatchService, IDocumentDBProvider documentDbProvider, IServiceBusClient serviceBusClient)
        {
            _actionPatchService = actionPatchService;
            _documentDbProvider = documentDbProvider;
            _serviceBusClient = serviceBusClient;
        }

        public string PatchResource(string actionJson, ActionPatch actionPatch)
        {
            if (string.IsNullOrEmpty(actionJson))
                return null;

            if (actionPatch == null)
                return null;

            actionPatch.SetDefaultValues();

            var updatedAction = _actionPatchService.Patch(actionJson, actionPatch);

            return updatedAction;
        }

        public async Task<Models.Action> UpdateCosmosAsync(string action, Guid actionId)
        {
            if (action == null)
                return null;

            var response = await _documentDbProvider.UpdateActionAsync(action, actionId);

            var responseStatusCode = response?.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? (dynamic)response.Resource : null;
        }

        public async Task<string> GetActionsForCustomerAsync(Guid customerId, Guid actionId, Guid actionPlanId)
        {
            return await _documentDbProvider.GetActionForCustomerToUpdateAsync(customerId, actionId, actionPlanId);
        }

        public async Task SendToServiceBusQueueAsync(Models.Action action, Guid customerId, string reqUrl)
        {
            await _serviceBusClient.SendPatchMessageAsync(action, customerId, reqUrl);
        }
    }
}