using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Action.Cosmos.Provider;
using NCS.DSS.Action.ServiceBus;

namespace NCS.DSS.Action.PostActionHttpTrigger.Service
{
    public class PostActionHttpTriggerService : IPostActionHttpTriggerService
    {
        private readonly IDocumentDBProvider _documentDbProvider;

        public PostActionHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<Models.Action> CreateAsync(Models.Action action)
        {
            if (action == null)
                return null;

            action.SetDefaultValues();

            var response = await _documentDbProvider.CreateActionAsync(action);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : null;
        }

        public async Task SendToServiceBusQueueAsync(Models.Action action, string reqUrl)
        {
            await ServiceBusClient.SendPostMessageAsync(action, reqUrl);
        }
    }
}