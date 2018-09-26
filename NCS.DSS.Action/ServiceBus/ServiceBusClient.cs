using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace NCS.DSS.Action.ServiceBus
{
    public static class ServiceBusClient
    {
        public static readonly string KeyName = ConfigurationManager.AppSettings["KeyName"];
        public static readonly string AccessKey = ConfigurationManager.AppSettings["AccessKey"];
        public static readonly string BaseAddress = ConfigurationManager.AppSettings["BaseAddress"];
        public static readonly string QueueName = ConfigurationManager.AppSettings["QueueName"];

        public static async Task SendPostMessageAsync(Models.Action action, string reqUrl)
        {
            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(KeyName, AccessKey);
            var messagingFactory = MessagingFactory.Create(BaseAddress, tokenProvider);
            var sender = messagingFactory.CreateMessageSender(QueueName);

            var messageModel = new MessageModel()
            {
                TitleMessage = "New Action record {" + action.ActionId + "} added at " + DateTime.UtcNow,
                CustomerGuid = action.CustomerId,
                LastModifiedDate = action.LastModifiedDate,
                URL = reqUrl + "/" + action.ActionId,
                IsNewCustomer = false,
                TouchpointId = action.LastModifiedTouchpointId
            };

            var msg = new BrokeredMessage(new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel))))
            {
                ContentType = "application/json",
                MessageId = action.CustomerId + " " + DateTime.UtcNow
            };

            //msg.ForcePersistence = true; Required when we save message to cosmos
            await sender.SendAsync(msg);
        }

        public static async Task SendPatchMessageAsync(Models.Action action, Guid customerId, string reqUrl)
        {
            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(KeyName, AccessKey);
            var messagingFactory = MessagingFactory.Create(BaseAddress, tokenProvider);
            var sender = messagingFactory.CreateMessageSender(QueueName);

            var messageModel = new MessageModel
            {
                TitleMessage = "Action record modification for {" + customerId + "} at " + DateTime.UtcNow,
                CustomerGuid = customerId,
                LastModifiedDate = action.LastModifiedDate,
                URL = reqUrl,
                IsNewCustomer = false,
                TouchpointId = action.LastModifiedTouchpointId
            };

            var msg = new BrokeredMessage(new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel))))
            {
                ContentType = "application/json",
                MessageId = customerId + " " + DateTime.UtcNow
            };

            //msg.ForcePersistence = true; Required when we save message to cosmos
            await sender.SendAsync(msg);
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

