using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Action.Models;
using System.Text;
using System.Text.Json;

namespace NCS.DSS.Action.ServiceBus
{
    public class ServiceBusClient : IServiceBusClient
    {
        private readonly ILogger<ServiceBusClient> _logger;
        private readonly ServiceBusSender _serviceBusSender;

        public ServiceBusClient(Azure.Messaging.ServiceBus.ServiceBusClient serviceBusClient, IOptions<ActionConfigurationSettings> configOptions, ILogger<ServiceBusClient> logger)
        {
            var config = configOptions.Value;
            if (string.IsNullOrEmpty(config.QueueName))
            {
                throw new ArgumentNullException(nameof(config.QueueName), "QueueName cannot be null or empty.");
            }

            _serviceBusSender = serviceBusClient.CreateSender(config.QueueName);
            _logger = logger;
        }

        public async Task SendPostMessageAsync(Models.Action action, string reqUrl)
        {
            try
            {
                _logger.LogTrace(
                    "Starting {MethodName}. Action ID: {ActionId}. Customer ID: {CustomerId}",
                    nameof(SendPostMessageAsync), action.ActionId, action.CustomerId);

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

                _logger.LogTrace(
                    "Completed {MethodName}. Action ID: {ActionId}. Customer ID: {CustomerId}",
                    nameof(SendPostMessageAsync), action.ActionId, action.CustomerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error occurred in {MethodName}. Action ID: {ActionId}. Customer ID: {CustomerId}",
                    nameof(SendPostMessageAsync), action.ActionId, action.CustomerId);
            }
        }

        public async Task SendPatchMessageAsync(Models.Action action, Guid customerId, string reqUrl)
        {
            try
            {
                _logger.LogTrace(
                    "Starting {MethodName}. Action ID: {ActionId}. Customer ID: {CustomerId}",
                    nameof(SendPostMessageAsync), action.ActionId, customerId);

                var messageModel = new MessageModel
                {
                    TitleMessage = "Action record modification for {" + customerId + "} at " + DateTime.UtcNow,
                    CustomerGuid = customerId,
                    LastModifiedDate = action.LastModifiedDate,
                    URL = reqUrl,
                    IsNewCustomer = false,
                    TouchpointId = action.LastModifiedTouchpointId
                };

                var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(messageModel)))
                {
                    ContentType = "application/json",
                    MessageId = messageModel.CustomerGuid + " " + DateTime.UtcNow
                };

                await _serviceBusSender.SendMessageAsync(msg);

                _logger.LogTrace(
                    "Completed {MethodName}. Action ID: {ActionId}. Customer ID: {CustomerId}",
                    nameof(SendPostMessageAsync), action.ActionId, action.CustomerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error occurred in {MethodName}. Action ID: {ActionId}. Customer ID: {CustomerId}",
                    nameof(SendPatchMessageAsync), action.ActionId, customerId);
            }
        }

        private async Task SendMessageToQueue(MessageModel messageModel)
        {
            try
            {
                var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(messageModel)))
                {
                    ContentType = "application/json",
                    MessageId = messageModel.CustomerGuid + " " + DateTime.UtcNow
                };

                var messageModelSerialized = JsonSerializer.Serialize(messageModel, new JsonSerializerOptions()
                {
                    WriteIndented = true
                });

                _logger.LogTrace(
                    "New Action record serialized: {MessageModel}. Customer GUID: {CustomerGuid}",
                    messageModelSerialized, messageModel.CustomerGuid);

                await _serviceBusSender.SendMessageAsync(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error occurred while sending message to queue. Customer GUID: {CustomerGuid}",
                    messageModel.CustomerGuid);
            }
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