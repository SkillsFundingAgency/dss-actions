using NCS.DSS.Action.Cosmos.Provider;

namespace NCS.DSS.Action.GetActionByIdHttpTrigger.Service
{
    public class GetActionByIdHttpTriggerService : IGetActionByIdHttpTriggerService
    {

        private readonly ICosmosDBProvider _cosmosDbProvider;

        public GetActionByIdHttpTriggerService(ICosmosDBProvider cosmosDbProvider)
        {
            _cosmosDbProvider = cosmosDbProvider;
        }

        public async Task<Models.Action> GetActionForCustomerAsync(Guid customerId, Guid actionId, Guid actionPlanId)
        {
            var action = await _cosmosDbProvider.GetActionForCustomerAsync(customerId, actionId, actionPlanId);

            return action;
        }
    }
}