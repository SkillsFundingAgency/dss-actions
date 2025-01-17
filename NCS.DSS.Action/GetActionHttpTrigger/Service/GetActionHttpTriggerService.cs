using NCS.DSS.Action.Cosmos.Provider;

namespace NCS.DSS.Action.GetActionHttpTrigger.Service
{
    public class GetActionHttpTriggerService : IGetActionHttpTriggerService
    {

        private readonly ICosmosDBProvider _cosmosDbProvider;

        public GetActionHttpTriggerService(ICosmosDBProvider cosmosDbProvider)
        {
            _cosmosDbProvider = cosmosDbProvider;
        }

        public async Task<List<Models.Action>> GetActionsAsync(Guid customerId, Guid actionPlanId)
        {
            var actions = await _cosmosDbProvider.GetActionsForCustomerAsync(customerId, actionPlanId);

            return actions;
        }
    }
}