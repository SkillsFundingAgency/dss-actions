using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace NCS.DSS.Action.ServiceBus
{
    public class ServiceBusClient : IServiceBusClient
    {
        public readonly string QueueName = Environment.GetEnvironmentVariable("QueueName");
        public readonly string ServiceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");

        public async Task SendPostMessageAsync(Models.Action action, string reqUrl)
        {
            var messageModel = new MessageModel()
            {
                TitleMessage = "New Action record {" + action.ActionId + "} added at " + DateTime.UtcNow,
                CustomerGuid = action.CustomerId,
                LastModifiedDate = action.LastModifiedDate,
                URL = reqUrl + "/" + action.ActionId,
                IsNewCustomer = false,
                TouchpointId = action.LastModifiedTouchpointId
            };

            await SendMessageToQueue(messageModel);
        }

        public async Task SendPatchMessageAsync(Models.Action action, Guid customerId, string reqUrl)
        {

            var messageModel = new MessageModel
            {
                TitleMessage = "Action record modification for {" + customerId + "} at " + DateTime.UtcNow,
                CustomerGuid = customerId,
                LastModifiedDate = action.LastModifiedDate,
                URL = reqUrl,
                IsNewCustomer = false,
                TouchpointId = action.LastModifiedTouchpointId
            };

            await SendMessageToQueue(messageModel);

        }

        private async Task SendMessageToQueue(MessageModel messageModel)
        {
            var queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = messageModel.CustomerGuid + " " + DateTime.UtcNow
            };

            await queueClient.SendAsync(msg);
        }
    }

    public class MessageModel
    {
        public string TitleMessage { get; set; }
        public Guid? CustomerGuid { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string URL { get; set; }
        public bool IsNewCustomer { get; set; }
        public string TouchpointId { get; set; }
    }

}