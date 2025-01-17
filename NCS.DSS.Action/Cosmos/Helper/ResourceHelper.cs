using NCS.DSS.Action.Cosmos.Provider;

namespace NCS.DSS.Action.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {

        private readonly ICosmosDBProvider _cosmosDbProvider;

        public ResourceHelper(ICosmosDBProvider cosmosDbProvider)
        {
            _cosmosDbProvider = cosmosDbProvider;
        }

        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            return await _cosmosDbProvider.DoesCustomerResourceExistAsync(customerId);
        }

        public async Task<bool> IsCustomerReadOnly(Guid customerId)
        {
            var isCustomerReadOnly = await _cosmosDbProvider.DoesCustomerHaveATerminationDateAsync(customerId);

            return isCustomerReadOnly;
        }

        public Task<bool> DoesInteractionExistAndBelongToCustomer(Guid interactionId, Guid customerGuid)
        {
            return _cosmosDbProvider.DoesInteractionResourceExistAndBelongToCustomerAsync(interactionId, customerGuid);
        }

        public Task<bool> DoesActionPlanExistAndBelongToCustomer(Guid actionPlanId, Guid interactionId, Guid customerId)
        {
            return _cosmosDbProvider.DoesActionPlanResourceExistAndBelongToCustomerAsync(actionPlanId, interactionId, customerId);
        }

    }
}