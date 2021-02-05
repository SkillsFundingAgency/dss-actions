using NCS.DSS.Action.ReferenceData;
using NUnit.Framework;
using System;

namespace NCS.DSS.Action.Tests.ModelTests
{
    [TestFixture]
    public class ActionTests
    {
        [Test]
        public void ActionTests_PopulatesDefaultValues_WhenSetDefaultValuesIsCalled()
        {
            // Arrange
            var action = new Action.Models.Action();

            // Act
            action.SetDefaultValues();

            // Assert
            Assert.NotNull(action.LastModifiedDate);
            Assert.AreEqual(ActionStatus.NotStarted, action.ActionStatus);
        }

        [Test]
        public void ActionTests_CheckLastModifiedDateDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            // Arrange
            var action = new Action.Models.Action { LastModifiedDate = DateTime.MaxValue };

            // Act
            action.SetDefaultValues();

            // Assert
            Assert.AreEqual(DateTime.MaxValue, action.LastModifiedDate);
        }

        [Test]
        public void ActionTests_CheckActionIdIsSet_WhenSetIdsIsCalled()
        {
            // Arrange
            var action = new Action.Models.Action();
            var actionPlanId = Guid.NewGuid();

            // Act
            action.SetIds(Guid.NewGuid(), actionPlanId, "0123456789");

            // Assert
            Assert.AreEqual(actionPlanId, action.ActionPlanId);
        }

        [Test]
        public void ActionTests_CheckCustomerIdIsSet_WhenSetIdsIsCalled()
        {
            // Arrange
            var action = new Action.Models.Action();
            var customerId = Guid.NewGuid();

            // Act
            action.SetIds(customerId, Guid.NewGuid(), "0123456789");

            // Assert
            Assert.AreEqual(customerId, action.CustomerId);
        }

        [Test]
        public void ActionTests_CheckLastModifiedTouchpointIdIsSet_WhenSetIdsIsCalled()
        {
            // Arrange
            var action = new Action.Models.Action();

            // Act
            action.SetIds(Guid.NewGuid(), Guid.NewGuid(), "0000000000");

            // Assert
            Assert.AreEqual("0000000000", action.LastModifiedTouchpointId);
        }

    }
}