using System;
using System.Threading.Tasks;
using NCS.DSS.Action.Cosmos.Provider;

namespace NCS.DSS.Action.GetActionByIdHttpTrigger.Service
{
    public class GetActionByIdHttpTriggerService : IGetActionByIdHttpTriggerService
    {

        private readonly IDocumentDBProvider _documentDbProvider;

        public GetActionByIdHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<Models.Action> GetActionForCustomerAsync(Guid customerId, Guid actionId)
        {
            var action = await _documentDbProvider.GetActionForCustomerAsync(customerId, actionId);

            return action;
        }
    }
}