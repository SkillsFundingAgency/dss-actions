using System;
using System.Threading.Tasks;

namespace NCS.DSS.Action.Cosmos.Helper
{
    public interface IResourceHelper
    {
        Task<bool> DoesCustomerExist(Guid customerId);
        Task<bool> IsCustomerReadOnly(Guid customerId);
        bool DoesInteractionExistAndBelongToCustomer(Guid interactionId, Guid customerGuid);
        bool DoesActionPlanExistAndBelongToCustomer(Guid actionPlanId, Guid interactionId, Guid customerId);
    }
}