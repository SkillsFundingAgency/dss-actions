using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using NCS.DSS.Action.Cosmos.Provider;
using NCS.DSS.Action.Models;
using NCS.DSS.Action.PatchActionHttpTrigger.Service;
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
    public class PatchActionHttpTriggerServiceTests
    {
        private IPatchActionHttpTriggerService _actionHttpTriggerService;
        private Mock<IActionPatchService> _actionPatchService;
        private Mock<IDocumentDBProvider> _documentDbProvider;
        private Mock<IServiceBusClient> _serviceBusClient;
        private string _json;
        private Models.Action _action;
        private ActionPatch _actionPatch;
        private readonly Guid _customerId = Guid.Parse("58b43e3f-4a50-4900-9c82-a14682ee90fa");
        private readonly Guid _actionId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");
        private readonly Guid _actionPlanId = Guid.Parse("12a16e3f-1c62-1660-3e81-b13122aa81aa");

        [SetUp]
        public void Setup()
        {
            _actionPatchService = new Mock<IActionPatchService>();
            _documentDbProvider = new Mock<IDocumentDBProvider>();
            _serviceBusClient = new Mock<IServiceBusClient>();
            _actionHttpTriggerService = new PatchActionHttpTriggerService(_actionPatchService.Object, _documentDbProvider.Object, _serviceBusClient.Object);
            _actionPatch = new ActionPatch();
            _action = new Models.Action();
            _json = JsonConvert.SerializeObject(_actionPatch);
            //_actionPatchService.Patch(_json, _actionPatch).Returns(_action.ToString());
        }

        [Test]
        public void PatchActionHttpTriggerServiceTests_PatchResource_ReturnsNullWhenActionJsonIsNullOrEmpty()
        {
            // Act
            var result = _actionHttpTriggerService.PatchResource(null, _actionPatch);

            // Assert
            Assert.Null(result);
        }

        [Test]
        public async Task PatchActionHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenResourceCannotBeUpdated()
        {
            // Arrange
            _documentDbProvider.Setup(x=>x.UpdateActionAsync(_json, _actionId)).Returns<string>(null);

            // Act
            var result = await _actionHttpTriggerService.UpdateCosmosAsync(_action.ToString(), _actionId);

            // Assert
            Assert.Null(result);
        }

        [Test]
        public async Task PatchActionHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            // Arrange
            _documentDbProvider.Setup(x=>x.CreateActionAsync(_action)).Returns(Task.FromResult(new ResourceResponse<Document>(null)));

            // Act
            var result = await _actionHttpTriggerService.UpdateCosmosAsync(_action.ToString(), _actionId);

            // Assert
            Assert.Null(result);
        }

        [Test]
        public async Task PatchActionPlanHttpTriggerServiceTests_UpdateAsync_ReturnsResourceWhenUpdated()
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

            var arguments = new[] { Stream.Null, headersDictionaryInstance, HttpStatusCode.OK, null };

            var documentServiceResponse = documentServiceResponseType.GetTypeInfo().GetConstructors(flags)[0].Invoke(arguments);

            var responseField = typeof(ResourceResponse<Document>).GetTypeInfo().GetField("response", flags);

            responseField?.SetValue(resourceResponse, documentServiceResponse);

            _documentDbProvider.Setup(x=>x.UpdateActionAsync(_json, _actionId)).Returns(Task.FromResult(resourceResponse));

            // Act
            var result = await _actionHttpTriggerService.UpdateCosmosAsync(_json, _actionId);

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<Models.Action>(result);

        }

        [Test]
        public async Task PatchActionHttpTriggerServiceTests_GetActionForCustomerAsync_ReturnsNullWhenResourceHasNotBeenFound()
        {
            // Arrange
            _documentDbProvider.Setup(x=>x.GetActionForCustomerToUpdateAsync(_customerId, _actionId, _actionPlanId)).Returns(Task.FromResult<string>(null));

            // Act
            var result = await _actionHttpTriggerService.GetActionsForCustomerAsync(_customerId, _actionId, _actionPlanId);

            // Assert
            Assert.Null(result);
        }

        [Test]
        public async Task PatchActionHttpTriggerServiceTests_GetActionForCustomerAsync_ReturnsResourceWhenResourceHasBeenFound()
        {
            // Arrange
            _documentDbProvider.Setup(x=>x.GetActionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(_action));
            _documentDbProvider.Setup(x => x.GetActionForCustomerToUpdateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult("some string"));

            // Act
            var result = await _actionHttpTriggerService.GetActionsForCustomerAsync(_customerId, _actionId, _actionPlanId);

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<string>(result);
        }
    }
}