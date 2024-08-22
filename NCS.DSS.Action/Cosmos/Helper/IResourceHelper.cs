namespace NCS.DSS.Action.Cosmos.Helper
{
    public interface IResourceHelper
    {
        Task<bool> DoesCustomerExist(Guid customerId);
        bool IsCustomerReadOnly();
        bool DoesInteractionExistAndBelongToCustomer(Guid interactionId, Guid customerGuid);
        bool DoesActionPlanExistAndBelongToCustomer(Guid actionPlanId, Guid interactionId, Guid customerId);
    }
}