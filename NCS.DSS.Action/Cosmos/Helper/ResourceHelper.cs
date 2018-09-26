using System;
using System.Threading.Tasks;
using NCS.DSS.Action.Cosmos.Provider;

namespace NCS.DSS.Action.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {
        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesCustomerExist = await documentDbProvider.DoesCustomerResourceExist(customerId);

            return doesCustomerExist;
        }

        public async Task<bool> IsCustomerReadOnly(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var isCustomerReadOnly = await documentDbProvider.DoesCustomerHaveATerminationDate(customerId);

            return isCustomerReadOnly;
        }

        public bool DoesInteractionExistAndBelongToCustomer(Guid interactionId, Guid customerGuid)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesInteractionExistAndBelongToCustomer = documentDbProvider.DoesInteractionResourceExistAndBelongToCustomer(interactionId, customerGuid);

            return doesInteractionExistAndBelongToCustomer;
        }

        public bool DoesActionPlanExistAndBelongToCustomer(Guid actionPlanId, Guid interactionId, Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesActionPlanExistAndBelongToCustomer = documentDbProvider.DoesActionPlanResourceExistAndBelongToCustomer(actionPlanId, interactionId, customerId);

            return doesActionPlanExistAndBelongToCustomer;
        }
    }
}
