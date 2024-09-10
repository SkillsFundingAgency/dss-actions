using NCS.DSS.Action.Cosmos.Provider;

namespace NCS.DSS.Action.GetActionHttpTrigger.Service
{
    public class GetActionHttpTriggerService : IGetActionHttpTriggerService
    {

        private readonly IDocumentDBProvider _documentDbProvider;

        public GetActionHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<List<Models.Action>> GetActionsAsync(Guid customerId, Guid actionPlanId)
        {
            var actions = await _documentDbProvider.GetActionsForCustomerAsync(customerId, actionPlanId);

            return actions;
        }
    }
}