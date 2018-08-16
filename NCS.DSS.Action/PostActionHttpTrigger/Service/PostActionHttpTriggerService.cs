using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Action.Cosmos.Provider;
using NCS.DSS.Action.ServiceBus;

namespace NCS.DSS.Action.PostActionHttpTrigger.Service
{
    public class PostActionHttpTriggerService : IPostActionHttpTriggerService
    {
        public async Task<Models.Action> CreateAsync(Models.Action action)
        {
            if (action == null)
                return null;

            action.SetDefaultValues();

            var documentDbProvider = new DocumentDBProvider();

            var response = await documentDbProvider.CreateActionAsync(action);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : null;
        }

        public async Task SendToServiceBusQueueAsync(Models.Action action, string reqUrl)
        {
            await ServiceBusClient.SendPostMessageAsync(action, reqUrl);
        }
    }
}