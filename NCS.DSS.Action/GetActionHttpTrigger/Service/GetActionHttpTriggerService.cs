using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NCS.DSS.Action.Cosmos.Provider;

namespace NCS.DSS.Action.GetActionHttpTrigger.Service
{
    public class GetActionHttpTriggerService : IGetActionHttpTriggerService
    {
        public async Task<List<Models.Action>> GetActionsAsync(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var actions = await documentDbProvider.GetActionsForCustomerAsync(customerId);

            return actions;
        }
    }
}
