using System;
using System.Threading.Tasks;
using NCS.DSS.Action.Cosmos.Provider;

namespace NCS.DSS.Action.GetActionByIdHttpTrigger.Service
{
    public class GetActionByIdHttpTriggerService : IGetActionByIdHttpTriggerService
    {
        public async Task<Models.Action> GetActionPlanForCustomerAsync(Guid customerId, Guid actionId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var action = await documentDbProvider.GetActionForCustomerAsync(customerId, actionId);

            return action;
        }
    }
}