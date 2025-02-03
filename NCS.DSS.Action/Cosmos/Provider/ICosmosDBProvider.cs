using Microsoft.Azure.Cosmos;

namespace NCS.DSS.Action.Cosmos.Provider
{
    public interface ICosmosDBProvider
    { 
        Task<bool> DoesCustomerResourceExistAsync(Guid customerId);
        Task<bool> DoesCustomerHaveATerminationDateAsync(Guid customerId);
        Task<bool> DoesInteractionResourceExistAndBelongToCustomerAsync(Guid interactionId, Guid customerId);
        Task<bool> DoesActionPlanResourceExistAndBelongToCustomerAsync(Guid actionPlanId, Guid interactionId, Guid customerId);
        Task<List<Models.Action>> GetActionsForCustomerAsync(Guid customerId, Guid actionPlanId);
        Task<Models.Action> GetActionForCustomerAsync(Guid customerId, Guid actionId, Guid actionPlanId);
        Task<string> GetActionForCustomerToUpdateAsync(Guid customerId, Guid actionId, Guid actionPlanId);
        Task<ItemResponse<Models.Action>> CreateActionAsync(Models.Action action);
        Task<ItemResponse<Models.Action>> UpdateActionAsync(string action, Guid actionId);
    }
}