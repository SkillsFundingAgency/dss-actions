using System;
using System.Threading.Tasks;
using NCS.DSS.Action.Models;

namespace NCS.DSS.Action.PatchActionHttpTrigger.Service
{
    public interface IPatchActionHttpTriggerService
    {
        string PatchResource(string actionJson, ActionPatch actionPatch);
        Task<Models.Action> UpdateCosmosAsync(string action, Guid actionId);
        Task<string> GetActionForCustomerAsync(Guid customerId, Guid actionId);
        Task SendToServiceBusQueueAsync(Models.Action action, Guid customerId, string reqUrl);
    }
}