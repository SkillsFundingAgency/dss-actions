using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NCS.DSS.Action.Cosmos.Provider;
using NCS.DSS.Action.GetActionHttpTrigger.Service;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Action.Tests.ServiceTests
{
    [TestFixture]
    public class GetActionHttpTriggerServiceTests
    {
        private IGetActionHttpTriggerService _actionHttpTriggerService;
        private IDocumentDBProvider _documentDbProvider;
        private readonly Guid _customerId = Guid.Parse("58b43e3f-4a50-4900-9c82-a14682ee90fa");
        private readonly Guid _actionPlanId = Guid.Parse("12a16e3f-1c62-1660-3e81-b13122aa81aa");

        [SetUp]
        public void Setup()
        {
            _documentDbProvider = Substitute.For<IDocumentDBProvider>();
            _actionHttpTriggerService = Substitute.For<GetActionHttpTriggerService>(_documentDbProvider);
        }

        [Test]
        public async Task GetActionHttpTriggerServiceTests_GetActionsAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            _documentDbProvider.GetActionsForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<List<Models.Action>>(null).Result);

            // Act
            var result = await _actionHttpTriggerService.GetActionsAsync(_customerId, _actionPlanId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetActionHttpTriggerServiceTests_GetActionsAsync_ReturnsResource()
        {
            _documentDbProvider.GetActionsForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(new List<Models.Action>()).Result);

            // Act
            var result = await _actionHttpTriggerService.GetActionsAsync(_customerId, _actionPlanId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<List<Models.Action>>(result);
        }
    }
}