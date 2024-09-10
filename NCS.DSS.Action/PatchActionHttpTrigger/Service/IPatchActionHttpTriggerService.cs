using NCS.DSS.Action.Models;

namespace NCS.DSS.Action.PatchActionHttpTrigger.Service
{
    public interface IPatchActionHttpTriggerService
    {
        string PatchResource(string actionJson, ActionPatch actionPatch);
        Task<Models.Action> UpdateCosmosAsync(string action, Guid actionId);
        Task<string> GetActionsForCustomerAsync(Guid customerId, Guid actionId, Guid actionPlanId);
        Task SendToServiceBusQueueAsync(Models.Action action, Guid customerId, string reqUrl);
    }
}