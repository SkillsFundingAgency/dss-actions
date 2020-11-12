using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using NCS.DSS.Action.Cosmos.Provider;
using NCS.DSS.Action.PostActionHttpTrigger.Service;
using NCS.DSS.Action.ServiceBus;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace NCS.DSS.Action.Tests.ServiceTests
{
    [TestFixture]
    public class PostActionHttpTriggerServiceTests
    {
        private IPostActionHttpTriggerService _actionHttpTriggerService;
        private Mock<IDocumentDBProvider> _documentDbProvider;
        private Mock<IServiceBusClient> _serviceBusClient;
        private string _json;
        private Models.Action _action;

        [SetUp]
        public void Setup()
        {
            _documentDbProvider = new Mock<IDocumentDBProvider>();
            _serviceBusClient = new Mock<IServiceBusClient>();
            _actionHttpTriggerService = new PostActionHttpTriggerService(_documentDbProvider.Object, _serviceBusClient.Object);
            _action = new Models.Action();
            _json = JsonConvert.SerializeObject(_action);
        }

        [Test]
        public async Task PostActionHttpTriggerServiceTests_CreateAsync_ReturnsNullWhenActionJsonIsNull()
        {
            // Act
            var result = await _actionHttpTriggerService.CreateAsync(null);

            // Assert
            Assert.Null(result);
        }

        [Test]
        public async Task PostActionHttpTriggerServiceTests_CreateAsync_ReturnsResource()
        {
            // Arrange
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

            _documentDbProvider.Setup(x=>x.CreateActionAsync(_action)).Returns(Task.FromResult(resourceResponse));

            // Act
            var result = await _actionHttpTriggerService.CreateAsync(_action);

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<Models.Action>(result);

        }
    }
}