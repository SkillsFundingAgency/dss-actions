using System;
using NCS.DSS.Action.ReferenceData;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Action.Tests.ModelTests
{
    [TestFixture]
    public class ActionTests
    {

        [Test]
        public void ActionTests_PopulatesDefaultValues_WhenSetDefaultValuesIsCalled()
        {
            var action = new Action.Models.Action();
            action.SetDefaultValues();

            // Assert
            Assert.IsNotNull(action.LastModifiedDate);
            Assert.AreEqual(ActionStatus.NotStarted, action.ActionStatus);
        }

        [Test]
        public void ActionTests_CheckLastModifiedDateDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            var action = new Action.Models.Action { LastModifiedDate = DateTime.MaxValue };

            action.SetDefaultValues();

            // Assert
            Assert.AreEqual(DateTime.MaxValue, action.LastModifiedDate);
        }

        [Test]
        public void ActionTests_CheckActionIdIsSet_WhenSetIdsIsCalled()
        {
            var action = new Action.Models.Action();

            action.SetIds(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());

            // Assert
            Assert.AreNotSame(Guid.Empty, action.ActionPlanId);
        }

        [Test]
        public void ActionTests_CheckCustomerIdIsSet_WhenSetIdsIsCalled()
        {
            var action = new Action.Models.Action();

            var customerId = Guid.NewGuid();
            action.SetIds(customerId, Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());

            // Assert
            Assert.AreEqual(customerId, action.CustomerId);
        }

        [Test]
        public void ActionTests_CheckActionPlanIdIsSet_WhenSetIdsIsCalled()
        {
            var action = new Action.Models.Action();

            var actionId = Guid.NewGuid();
            action.SetIds(Arg.Any<Guid>(), actionId, Arg.Any<string>(), Arg.Any<string>());

            // Assert
            Assert.AreEqual(actionId, action.ActionPlanId);
        }

        [Test]
        public void ActionTests_CheckLastModifiedTouchpointIdIsSet_WhenSetIdsIsCalled()
        {
            var action = new Action.Models.Action();

            action.SetIds(Arg.Any<Guid>(), Arg.Any<Guid>(), "0000000000", Arg.Any<string>());

            // Assert
            Assert.AreEqual("0000000000", action.LastModifiedTouchpointId);
        }

    }
}