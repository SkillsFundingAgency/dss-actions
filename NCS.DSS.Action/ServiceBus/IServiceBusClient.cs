using System;
using System.Threading.Tasks;
using NCS.DSS.Action.Models;

namespace NCS.DSS.Action.ServiceBus
{
    public interface IServiceBusClient
    {
        Task SendPostMessageAsync(Models.Action action, string reqUrl);
        Task SendPatchMessageAsync(Models.Action action, Guid customerId, string reqUrl);
    }
}
