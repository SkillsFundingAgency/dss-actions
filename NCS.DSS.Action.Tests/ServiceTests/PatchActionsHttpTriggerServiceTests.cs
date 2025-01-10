using Microsoft.Azure.Cosmos;
using Moq;
using NCS.DSS.Action.Cosmos.Provider;
using NCS.DSS.Action.Models;
using NCS.DSS.Action.PatchActionHttpTrigger.Service;
using NCS.DSS.Action.ServiceBus;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.Action.Tests.ServiceTests
{
    [TestFixture]
    public class PatchActionHttpTriggerServiceTests
    {
        private IPatchActionHttpTriggerService _actionHttpTriggerService;
        private Mock<IActionPatchService> _actionPatchService;
        private Mock<ICosmosDBProvider> _documentDbProvider;
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
            _documentDbProvider = new Mock<ICosmosDBProvider>();
            _serviceBusClient = new Mock<IServiceBusClient>();

            _actionHttpTriggerService = new PatchActionHttpTriggerService(_actionPatchService.Object, _documentDbProvider.Object, _serviceBusClient.Object);
            _actionPatch = new ActionPatch();
            _action = new Models.Action();
            _json = JsonConvert.SerializeObject(_actionPatch);
        }

        [Test]
        public void PatchActionHttpTriggerServiceTests_PatchResource_ReturnsNullWhenActionJsonIsNullOrEmpty()
        {
            // Act
            var result = _actionHttpTriggerService.PatchResource(null, _actionPatch);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PatchActionHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenResourceCannotBeUpdated()
        {
            // Arrange
            _documentDbProvider.Setup(x => x.UpdateActionAsync(_json, _actionId)).Returns<string>(null);

            // Act
            var result = await _actionHttpTriggerService.UpdateCosmosAsync(_action.ToString(), _actionId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PatchActionHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            // Arrange
            var mockItemResponse = new Mock<ItemResponse<Models.Action>>();
    
            mockItemResponse.Setup(x => x.Resource).Returns((Models.Action)null);
            mockItemResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.NotFound);

            _documentDbProvider
                .Setup(x => x.UpdateActionAsync(It.IsAny<string>(), It.IsAny<Guid>()))
                .ReturnsAsync(mockItemResponse.Object);

            // Act
            var result = await _actionHttpTriggerService.UpdateCosmosAsync(_action.ToString(), _actionId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PatchActionPlanHttpTriggerServiceTests_UpdateAsync_ReturnsResourceWhenUpdated()
        {
            // Arrange
            var mockItemResponse = new Mock<ItemResponse<Models.Action>>();
    
            mockItemResponse.Setup(x => x.Resource).Returns(_action);
            mockItemResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);

            _documentDbProvider
                .Setup(x => x.UpdateActionAsync(_json, _actionId))
                .ReturnsAsync(mockItemResponse.Object);

            // Act
            var result = await _actionHttpTriggerService.UpdateCosmosAsync(_json, _actionId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Models.Action>());
        }

        [Test]
        public async Task PatchActionHttpTriggerServiceTests_GetActionForCustomerAsync_ReturnsNullWhenResourceHasNotBeenFound()
        {
            // Arrange
            _documentDbProvider.Setup(x => x.GetActionForCustomerToUpdateAsync(_customerId, _actionId, _actionPlanId)).Returns(Task.FromResult<string>(null));

            // Act
            var result = await _actionHttpTriggerService.GetActionsForCustomerAsync(_customerId, _actionId, _actionPlanId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PatchActionHttpTriggerServiceTests_GetActionForCustomerAsync_ReturnsResourceWhenResourceHasBeenFound()
        {
            // Arrange
            _documentDbProvider.Setup(x => x.GetActionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(_action));
            _documentDbProvider.Setup(x => x.GetActionForCustomerToUpdateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult("some string"));

            // Act
            var result = await _actionHttpTriggerService.GetActionsForCustomerAsync(_customerId, _actionId, _actionPlanId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<string>());
        }
    }
}