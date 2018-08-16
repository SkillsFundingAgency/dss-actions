using System;
using System.Threading.Tasks;

namespace NCS.DSS.Action.PatchActionHttpTrigger.Service
{
    public interface IPatchActionHttpTriggerService
    {
        Task<Models.Action> UpdateAsync(Models.Action action, Models.ActionPatch actionPatch);
        Task<Models.Action> GetActionForCustomerAsync(Guid customerId, Guid actionId);
        Task SendToServiceBusQueueAsync(Models.Action action, Guid customerId, string reqUrl);
    }
}