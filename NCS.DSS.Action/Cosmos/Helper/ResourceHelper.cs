using NCS.DSS.Action.Cosmos.Provider;

namespace NCS.DSS.Action.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {

        private readonly ICosmosDBProvider _documentDbProvider;

        public ResourceHelper(ICosmosDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            return await _documentDbProvider.DoesCustomerResourceExistAsync(customerId);
        }

        public async Task<bool> IsCustomerReadOnly(Guid customerId)
        {
            var isCustomerReadOnly = await _documentDbProvider.DoesCustomerHaveATerminationDateAsync(customerId);

            return isCustomerReadOnly;
        }

        public Task<bool> DoesInteractionExistAndBelongToCustomer(Guid interactionId, Guid customerGuid)
        {
            return _documentDbProvider.DoesInteractionResourceExistAndBelongToCustomerAsync(interactionId, customerGuid);
        }

        public Task<bool> DoesActionPlanExistAndBelongToCustomer(Guid actionPlanId, Guid interactionId, Guid customerId)
        {
            return _documentDbProvider.DoesActionPlanResourceExistAndBelongToCustomerAsync(actionPlanId, interactionId, customerId);
        }

    }
}