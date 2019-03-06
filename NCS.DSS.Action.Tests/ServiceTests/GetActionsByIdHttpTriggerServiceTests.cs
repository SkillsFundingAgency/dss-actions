﻿using System;
using System.Threading.Tasks;
using NCS.DSS.Action.Cosmos.Provider;
using NCS.DSS.Action.GetActionByIdHttpTrigger.Service;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace NCS.DSS.Action.Tests.ServiceTests
{
    [TestFixture]
    public class GetActionByIdHttpTriggerServiceTests
    {
        private IGetActionByIdHttpTriggerService _actionHttpTriggerService;
        private IDocumentDBProvider _documentDbProvider;
        private Action.Models.Action _action;
        private readonly Guid _actionId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");
        private readonly Guid _customerId = Guid.Parse("58b43e3f-4a50-4900-9c82-a14682ee90fa");


        [SetUp]
        public void Setup()
        {
            _documentDbProvider = Substitute.For<IDocumentDBProvider>();
            _actionHttpTriggerService = Substitute.For<GetActionByIdHttpTriggerService>(_documentDbProvider);
            _action = Substitute.For<Action.Models.Action>();
        }

        [Test]
        public async Task GetActionHttpTriggerServiceTests_GetActionForCustomerAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            _documentDbProvider.GetActionForCustomerAsync(_customerId,_actionId).ReturnsNull();

            // Act
            var result = await _actionHttpTriggerService.GetActionForCustomerAsync(_customerId, _actionId);

            // Assert
            Assert.IsNull(result);
        }
    }
}