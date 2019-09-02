using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NCS.DSS.Action.Cosmos.Provider;
using NCS.DSS.Action.GetActionHttpTrigger.Service;
using NSubstitute;
using Xunit;

namespace NCS.DSS.Action.Tests.ServiceTests
{

    public class GetActionHttpTriggerServiceTests
    {
        private readonly IGetActionHttpTriggerService _actionHttpTriggerService;
        private readonly IDocumentDBProvider _documentDbProvider;
        private readonly Guid _customerId = Guid.Parse("58b43e3f-4a50-4900-9c82-a14682ee90fa");
        private readonly Guid _actionPlanId = Guid.Parse("12a16e3f-1c62-1660-3e81-b13122aa81aa");

        public GetActionHttpTriggerServiceTests()
        {
            _documentDbProvider = Substitute.For<IDocumentDBProvider>();
            _actionHttpTriggerService = Substitute.For<GetActionHttpTriggerService>(_documentDbProvider);
        }

        [Fact]
        public async Task GetActionHttpTriggerServiceTests_GetActionsAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            _documentDbProvider.GetActionsForCustomerAsync(_customerId, _actionPlanId).Returns(Task.FromResult<List<Models.Action>>(null).Result);

            // Act
            var result = await _actionHttpTriggerService.GetActionsAsync(_customerId, _actionPlanId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetActionHttpTriggerServiceTests_GetActionsAsync_ReturnsResource()
        {
            _documentDbProvider.GetActionsForCustomerAsync(_customerId, _actionPlanId).Returns(Task.FromResult(new List<Models.Action>()).Result);

            // Act
            var result = await _actionHttpTriggerService.GetActionsAsync(_customerId, _actionPlanId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<Models.Action>>(result);
        }
    }
}