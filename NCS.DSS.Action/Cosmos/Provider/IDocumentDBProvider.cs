using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Action.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        string GetCustomerJson();
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        bool DoesInteractionResourceExistAndBelongToCustomer(Guid interactionId, Guid customerId);
        bool DoesActionPlanResourceExistAndBelongToCustomer(Guid actionPlanId, Guid interactionId, Guid customerId);
        Task<List<Models.Action>> GetActionsForCustomerAsync(Guid customerId, Guid actionPlanId);
        Task<Models.Action> GetActionForCustomerAsync(Guid customerId, Guid actionId, Guid actionPlanId);
        Task<string> GetActionForCustomerToUpdateAsync(Guid customerId, Guid actionId, Guid actionPlanId);
        Task<ResourceResponse<Document>> CreateActionAsync(Models.Action action);
        Task<ResourceResponse<Document>> UpdateActionAsync(string action, Guid actionId);
    }
}