using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Action.Cosmos.Provider;
using NCS.DSS.Action.Models;

namespace NCS.DSS.Action.PatchActionHttpTrigger.Service
{
    public class PatchActionHttpTriggerService : IPatchActionHttpTriggerService
    {
        public async Task<Models.Action> UpdateAsync(Models.Action action, ActionPatch actionPatch)
        {
            if (action == null)
                return null;

            if (!actionPatch.LastModifiedDate.HasValue)
                actionPatch.LastModifiedDate = DateTime.Now;

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
    }
}