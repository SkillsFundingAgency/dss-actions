using DFC.JSON.Standard;
using NCS.DSS.Action.Cosmos.Provider;

namespace NCS.DSS.Action.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {

        private readonly IDocumentDBProvider _documentDbProvider;
        private readonly IJsonHelper _jsonHelper;

        public ResourceHelper(IDocumentDBProvider documentDbProvider, IJsonHelper jsonHelper)
        {
            _documentDbProvider = documentDbProvider;
            _jsonHelper = jsonHelper;
        }

        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            return await _documentDbProvider.DoesCustomerResourceExist(customerId);
        }

        public bool IsCustomerReadOnly()
        {
            var customerJson = _documentDbProvider.GetCustomerJson();

            if (string.IsNullOrWhiteSpace(customerJson))
                return false;

            var dateOfTermination = _jsonHelper.GetValue(customerJson, "DateOfTermination");

            return !string.IsNullOrWhiteSpace(dateOfTermination);
        }

        public bool DoesInteractionExistAndBelongToCustomer(Guid interactionId, Guid customerGuid)
        {
            return _documentDbProvider.DoesInteractionResourceExistAndBelongToCustomer(interactionId, customerGuid);
        }

        public bool DoesActionPlanExistAndBelongToCustomer(Guid actionPlanId, Guid interactionId, Guid customerId)
        {
            return _documentDbProvider.DoesActionPlanResourceExistAndBelongToCustomer(actionPlanId, interactionId, customerId);
        }

    }
}