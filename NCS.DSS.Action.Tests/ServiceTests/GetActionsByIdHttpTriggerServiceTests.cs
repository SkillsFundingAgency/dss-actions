using System;
using System.Threading.Tasks;
using NCS.DSS.Action.Cosmos.Provider;
using NCS.DSS.Action.GetActionByIdHttpTrigger.Service;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace NCS.DSS.Action.Tests.ServiceTests
{

    public class GetActionByIdHttpTriggerServiceTests
    {
        private readonly IGetActionByIdHttpTriggerService _actionHttpTriggerService;
        private readonly IDocumentDBProvider _documentDbProvider;
        private readonly Guid _actionId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");
        private readonly Guid _customerId = Guid.Parse("58b43e3f-4a50-4900-9c82-a14682ee90fa");
        private readonly Guid _actionPlanId = Guid.Parse("12a16e3f-1c62-1660-3e81-b13122aa81aa");


        public GetActionByIdHttpTriggerServiceTests()
        {
            _documentDbProvider = Substitute.For<IDocumentDBProvider>();
            _actionHttpTriggerService = Substitute.For<GetActionByIdHttpTriggerService>(_documentDbProvider);
        }

        [Fact]
        public async Task GetActionHttpTriggerServiceTests_GetActionForCustomerAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            _documentDbProvider.GetActionForCustomerAsync(_customerId,_actionId, _actionPlanId).ReturnsNull();

            // Act
            var result = await _actionHttpTriggerService.GetActionForCustomerAsync(_customerId, _actionId, _actionPlanId);

            // Assert
            Assert.Null(result);
        }
    }
}