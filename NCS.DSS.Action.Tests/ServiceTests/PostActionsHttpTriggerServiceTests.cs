using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using NCS.DSS.Action.Cosmos.Provider;
using NCS.DSS.Action.PostActionHttpTrigger.Service;
using NCS.DSS.Action.ServiceBus;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;

namespace NCS.DSS.Action.Tests.ServiceTests
{
    public class PostActionHttpTriggerServiceTests
    {
        private readonly IPostActionHttpTriggerService _actionHttpTriggerService;
        private readonly IDocumentDBProvider _documentDbProvider;
        private readonly IServiceBusClient _serviceBusClient;

        private readonly string _json;
        private readonly Models.Action _action;

        public PostActionHttpTriggerServiceTests()
        {
            _documentDbProvider = Substitute.For<IDocumentDBProvider>();
            _serviceBusClient = Substitute.For<IServiceBusClient>();

            _actionHttpTriggerService = Substitute.For<PostActionHttpTriggerService>(_documentDbProvider, _serviceBusClient);
            _action = Substitute.For<Models.Action>();
            _json = JsonConvert.SerializeObject(_action);
        }

        [Fact]
        public async Task PostActionHttpTriggerServiceTests_CreateAsync_ReturnsNullWhenActionJsonIsNull()
        {
            // Act
            var result = await _actionHttpTriggerService.CreateAsync(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task PostActionHttpTriggerServiceTests_CreateAsync_ReturnsResource()
        {
            const string documentServiceResponseClass = "Microsoft.Azure.Documents.DocumentServiceResponse, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
            const string dictionaryNameValueCollectionClass = "Microsoft.Azure.Documents.Collections.DictionaryNameValueCollection, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

            var resourceResponse = new ResourceResponse<Document>(new Document());
            var documentServiceResponseType = Type.GetType(documentServiceResponseClass);

            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var headers = new NameValueCollection { { "x-ms-request-charge", "0" } };

            var headersDictionaryType = Type.GetType(dictionaryNameValueCollectionClass);

            var headersDictionaryInstance = Activator.CreateInstance(headersDictionaryType, headers);

            var arguments = new[] { Stream.Null, headersDictionaryInstance, HttpStatusCode.Created, null };

            var documentServiceResponse = documentServiceResponseType.GetTypeInfo().GetConstructors(flags)[0].Invoke(arguments);

            var responseField = typeof(ResourceResponse<Document>).GetTypeInfo().GetField("response", flags);

            responseField?.SetValue(resourceResponse, documentServiceResponse);

            _documentDbProvider.CreateActionAsync(_action).Returns(Task.FromResult(resourceResponse).Result);

            // Act
            var result = await _actionHttpTriggerService.CreateAsync(_action);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Models.Action>(result);

        }
    }
}