using System.Threading.Tasks;

namespace NCS.DSS.Action.PostActionHttpTrigger.Service
{
    public interface IPostActionHttpTriggerService
    {
        Task<Models.Action> CreateAsync(Models.Action action);
        Task SendToServiceBusQueueAsync(Models.Action action, string reqUrl);
    }
}