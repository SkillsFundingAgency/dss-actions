using Microsoft.Azure.Cosmos;
using Moq;
using NCS.DSS.Action.Cosmos.Provider;
using NCS.DSS.Action.PostActionHttpTrigger.Service;
using NCS.DSS.Action.ServiceBus;
using NUnit.Framework;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.Action.Tests.ServiceTests
{
    [TestFixture]
    public class PostActionHttpTriggerServiceTests
    {
        private IPostActionHttpTriggerService _actionHttpTriggerService;
        private Mock<ICosmosDBProvider> _cosmosDbProvider;
        private Mock<IServiceBusClient> _serviceBusClient;
        private Models.Action _action;

        [SetUp]
        public void Setup()
        {
            _cosmosDbProvider = new Mock<ICosmosDBProvider>();
            _serviceBusClient = new Mock<IServiceBusClient>();

            _actionHttpTriggerService = new PostActionHttpTriggerService(_cosmosDbProvider.Object, _serviceBusClient.Object);
            _action = new Models.Action();
        }

        [Test]
        public async Task PostActionHttpTriggerServiceTests_CreateAsync_ReturnsNullWhenActionJsonIsNull()
        {
            // Act
            var result = await _actionHttpTriggerService.CreateAsync(null);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PostActionHttpTriggerServiceTests_CreateAsync_ReturnsResource()
        {
            // Arrange
            var mockItemResponse = new Mock<ItemResponse<Models.Action>>();

            mockItemResponse.Setup(x => x.Resource).Returns(_action);
            mockItemResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.Created);

            _cosmosDbProvider
                .Setup(x => x.CreateActionAsync(_action))
                .ReturnsAsync(mockItemResponse.Object);

            // Act
            var result = await _actionHttpTriggerService.CreateAsync(_action);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Models.Action>());
        }
    }
}