using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Action.Cosmos.Provider;

namespace NCS.DSS.Action.PostActionHttpTrigger.Service
{
    public class PostActionHttpTriggerService : IPostActionHttpTriggerService
    {
        public async Task<Models.Action> CreateAsync(Models.Action action)
        {
            if (action == null)
                return null;

            var actionPlanId = Guid.NewGuid();
            action.ActionPlanId = actionPlanId;

            if (!action.LastModifiedDate.HasValue)
                action.LastModifiedDate = DateTime.Now;

            var documentDbProvider = new DocumentDBProvider();

            var response = await documentDbProvider.CreateActionAsync(action);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : null;
        }
    }
}