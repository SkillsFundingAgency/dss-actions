using Moq;
using NCS.DSS.Action.Cosmos.Provider;
using NCS.DSS.Action.GetActionHttpTrigger.Service;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.Action.Tests.ServiceTests
{
    [TestFixture]
    public class GetActionHttpTriggerServiceTests
    {
        private readonly IGetActionHttpTriggerService _actionHttpTriggerService;
        private readonly Mock<IDocumentDBProvider> _documentDbProvider;
        private readonly Guid _customerId = Guid.Parse("58b43e3f-4a50-4900-9c82-a14682ee90fa");
        private readonly Guid _actionPlanId = Guid.Parse("12a16e3f-1c62-1660-3e81-b13122aa81aa");

        public GetActionHttpTriggerServiceTests()
        {
            _documentDbProvider = new Mock<IDocumentDBProvider>();
            _actionHttpTriggerService = new GetActionHttpTriggerService(_documentDbProvider.Object);
        }

        [Test]
        public async Task GetActionHttpTriggerServiceTests_GetActionsAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            // Arrange
            _documentDbProvider.Setup(x => x.GetActionsForCustomerAsync(_customerId, _actionPlanId)).Returns(Task.FromResult<List<Models.Action>>(null));

            // Act
            var result = await _actionHttpTriggerService.GetActionsAsync(_customerId, _actionPlanId);

            // Assert
            Assert.That(result,Is.Null);
        }

        [Test]
        public async Task GetActionHttpTriggerServiceTests_GetActionsAsync_ReturnsResource()
        {
            // Arrange
            _documentDbProvider.Setup(x => x.GetActionsForCustomerAsync(_customerId, _actionPlanId)).Returns(Task.FromResult(new List<Models.Action>()));

            // Act
            var result = await _actionHttpTriggerService.GetActionsAsync(_customerId, _actionPlanId);

            // Assert
            Assert.That(result,Is.Not.Null);
            Assert.That(result, Is.InstanceOf<List<Models.Action>>());
        }
    }
}