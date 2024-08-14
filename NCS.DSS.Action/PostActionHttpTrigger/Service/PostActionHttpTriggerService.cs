using NCS.DSS.Action.Cosmos.Provider;
using NCS.DSS.Action.ServiceBus;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.Action.PostActionHttpTrigger.Service
{
    public class PostActionHttpTriggerService : IPostActionHttpTriggerService
    {
        private readonly IDocumentDBProvider _documentDbProvider;
        private readonly IServiceBusClient _serviceBusClient;

        public PostActionHttpTriggerService(IDocumentDBProvider documentDbProvider, IServiceBusClient serviceBusClient)
        {
            _documentDbProvider = documentDbProvider;
            _serviceBusClient = serviceBusClient;
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
            await _serviceBusClient.SendPostMessageAsync(action, reqUrl);
        }
    }
}