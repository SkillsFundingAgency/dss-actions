using System;
using NCS.DSS.Action.ReferenceData;
using NSubstitute;
using Xunit;

namespace NCS.DSS.Action.Tests.ModelTests
{
    public class ActionTests
    {

        [Fact]
        public void ActionTests_PopulatesDefaultValues_WhenSetDefaultValuesIsCalled()
        {
            var action = new Action.Models.Action();
            action.SetDefaultValues();

            // Assert
            Assert.NotNull(action.LastModifiedDate);
            Assert.Equal(ActionStatus.NotStarted, action.ActionStatus);
        }

        [Fact]
        public void ActionTests_CheckLastModifiedDateDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            var action = new Action.Models.Action { LastModifiedDate = DateTime.MaxValue };

            action.SetDefaultValues();

            // Assert
            Assert.Equal(DateTime.MaxValue, action.LastModifiedDate);
        }

        [Fact]
        public void ActionTests_CheckActionIdIsSet_WhenSetIdsIsCalled()
        {
            var action = new Action.Models.Action();
            action.SetIds(Guid.NewGuid(), Guid.NewGuid(), "0123456789");

            // Assert
            Assert.NotEqual(Guid.Empty, action.ActionPlanId);
        }

        [Fact]
        public void ActionTests_CheckCustomerIdIsSet_WhenSetIdsIsCalled()
        {
            var action = new Action.Models.Action();

            action.SetIds(Guid.NewGuid(), Guid.NewGuid(), "0123456789");

            // Assert
            Assert.NotEqual(Guid.Empty, action.CustomerId);
        }

        [Fact]
        public void ActionTests_CheckActionPlanIdIsSet_WhenSetIdsIsCalled()
        {
            var action = new Action.Models.Action();

            action.SetIds(Guid.NewGuid(), Guid.NewGuid(), "0123456789");

            // Assert
            Assert.NotEqual(Guid.Empty, action.ActionPlanId);
        }

        [Fact]
        public void ActionTests_CheckLastModifiedTouchpointIdIsSet_WhenSetIdsIsCalled()
        {
            var action = new Action.Models.Action();

            action.SetIds(Guid.NewGuid(), Guid.NewGuid(), "0000000000");

            // Assert
            Assert.Equal("0000000000", action.LastModifiedTouchpointId);
        }

    }
}