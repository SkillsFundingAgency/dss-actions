using NCS.DSS.Action.Cosmos.Provider;
using System;
using System.Threading.Tasks;

namespace NCS.DSS.Action.GetActionByIdHttpTrigger.Service
{
    public class GetActionByIdHttpTriggerService : IGetActionByIdHttpTriggerService
    {

        private readonly IDocumentDBProvider _documentDbProvider;

        public GetActionByIdHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<Models.Action> GetActionForCustomerAsync(Guid customerId, Guid actionId, Guid actionPlanId)
        {
            var action = await _documentDbProvider.GetActionForCustomerAsync(customerId, actionId, actionPlanId);

            return action;
        }
    }
}