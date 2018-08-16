using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Action.Cosmos.Provider;
using NCS.DSS.Action.Models;
using NCS.DSS.Action.ServiceBus;

namespace NCS.DSS.Action.PatchActionHttpTrigger.Service
{
    public class PatchActionHttpTriggerService : IPatchActionHttpTriggerService
    {
        public async Task<Models.Action> UpdateAsync(Models.Action action, ActionPatch actionPatch)
        {
            if (action == null)
                return null;

            actionPatch.SetDefaultValues();

            action.Patch(actionPatch);

            var documentDbProvider = new DocumentDBProvider();
            var response = await documentDbProvider.UpdateActionAsync(action);

            var responseStatusCode = response.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? action : null;
        }

        public async Task<Models.Action> GetActionForCustomerAsync(Guid customerId, Guid actionId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var action = await documentDbProvider.GetActionForCustomerAsync(customerId, actionId);

            return action;
        }

        public async Task SendToServiceBusQueueAsync(Models.Action action, Guid customerId, string reqUrl)
        {
            await ServiceBusClient.SendPatchMessageAsync(action, customerId, reqUrl);
        }
    }
}