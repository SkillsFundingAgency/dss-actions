using NCS.DSS.Action.Cosmos.Provider;
using NCS.DSS.Action.Models;
using NCS.DSS.Action.ServiceBus;
using System.Net;

namespace NCS.DSS.Action.PatchActionHttpTrigger.Service
{
    public class PatchActionHttpTriggerService : IPatchActionHttpTriggerService
    {
        private readonly IActionPatchService _actionPatchService;
        private readonly ICosmosDBProvider _cosmosDbProvider;
        private readonly IServiceBusClient _serviceBusClient;

        public PatchActionHttpTriggerService(IActionPatchService actionPatchService, ICosmosDBProvider cosmosDbProvider, IServiceBusClient serviceBusClient)
        {
            _actionPatchService = actionPatchService;
            _cosmosDbProvider = cosmosDbProvider;
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

            var response = await _cosmosDbProvider.UpdateActionAsync(action, actionId);

            var responseStatusCode = response?.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? (dynamic)response.Resource : null;
        }

        public async Task<string> GetActionsForCustomerAsync(Guid customerId, Guid actionId, Guid actionPlanId)
        {
            return await _cosmosDbProvider.GetActionForCustomerToUpdateAsync(customerId, actionId, actionPlanId);
        }

        public async Task SendToServiceBusQueueAsync(Models.Action action, Guid customerId, string reqUrl)
        {
            await _serviceBusClient.SendPatchMessageAsync(action, customerId, reqUrl);
        }
    }
}