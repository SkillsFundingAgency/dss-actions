using System;
using NCS.DSS.Action.Helpers;
using NCS.DSS.Action.Models;
using NCS.DSS.Action.PatchActionHttpTrigger.Service;
using NCS.DSS.Action.ReferenceData;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.action.Tests.ServiceTests
{
    [TestFixture]
    public class ActionPatchServiceTests
    {
        private JsonHelper _jsonHelper;
        private IActionPatchService _actionPatchService;
        private ActionPatch _actionPatch;
        private string _json;


        [SetUp]
        public void Setup()
        {
            _jsonHelper = Substitute.For<JsonHelper>();
            _actionPatchService = Substitute.For<ActionPatchService>();
            _actionPatch = Substitute.For<ActionPatch>();

            _json = JsonConvert.SerializeObject(_actionPatch);
        }

        [Test]
        public void ActionPatchServiceTests_ReturnsNull_WhenActionPatchIsNull()
        {
            var result = _actionPatchService.Patch(string.Empty, Arg.Any<ActionPatch>());

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void ActionPatchServiceTests_CheckDateActionAgreedIsUpdated_WhenPatchIsCalled()
        {
            var actionPatch = new ActionPatch() {DateActionAgreed = DateTime.MaxValue};

            var patchedAction = _actionPatchService.Patch(_json, actionPatch);

            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedAction);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, action.DateActionAgreed);
        }
        

        [Test]
        public void ActionPatchServiceTests_CheckDateActionAimsToBeCompletedByIsUpdated_WhenPatchIsCalled()
        {
            var actionPatch = new ActionPatch {DateActionAimsToBeCompletedBy = DateTime.MaxValue};

            var patchedAction = _actionPatchService.Patch(_json, actionPatch);

            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedAction);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, action.DateActionAimsToBeCompletedBy);
        }

        [Test]
        public void ActionPatchServiceTests_CheckDateActionActuallyCompletedIsUpdated_WhenPatchIsCalled()
        {
            var actionPatch = new ActionPatch {DateActionActuallyCompleted = DateTime.MaxValue};

            var patchedAction = _actionPatchService.Patch(_json, actionPatch);

            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedAction);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, action.DateActionActuallyCompleted);
        }

        [Test]
        public void ActionPatchServiceTests_CheckActionSummaryIsUpdated_WhenPatchIsCalled()
        {
            var actionPatch = new ActionPatch {ActionSummary = "TestActionSummary"};

            var patchedAction = _actionPatchService.Patch(_json, actionPatch);

            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedAction);

            // Assert
            Assert.AreEqual("TestActionSummary", action.ActionSummary);
        }

        [Test]
        public void ActionPatchServiceTests_CheckSignpostedToMethodIsUpdated_WhenPatchIsCalled()
        {
            var actionPatch = new ActionPatch {SignpostedTo = "SignpostedToTest"};

            var patchedAction = _actionPatchService.Patch(_json, actionPatch);

            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedAction);

            // Assert
            Assert.AreEqual("SignpostedToTest", action.SignpostedTo);
        }

        [Test]
        public void ActionPatchServiceTests_CheckActionTypeIsUpdated_WhenPatchIsCalled()
        {
            var actionPatch = new ActionPatch {ActionType = DSS.Action.ReferenceData.ActionType.ApplyForApprenticeship};

            var patchedAction = _actionPatchService.Patch(_json, actionPatch);

            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedAction);

            // Assert
            Assert.AreEqual(ActionType.ApplyForApprenticeship, action.ActionType);
        }

        [Test]
        public void ActionPatchServiceTests_CheckActionStatusIsUpdated_WhenPatchIsCalled()
        {
            var actionPatch = new ActionPatch {ActionStatus = ActionStatus.InProgress};

            var patchedAction = _actionPatchService.Patch(_json, actionPatch);

            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedAction);

            // Assert
            Assert.AreEqual(ActionStatus.InProgress, action.ActionStatus);
        }

        [Test]
        public void ActionPatchServiceTests_CheckPersonResponsibleIsUpdated_WhenPatchIsCalled()
        {
            var actionPatch = new ActionPatch {PersonResponsible = PersonResponsible.Adviser};

            var patchedAction = _actionPatchService.Patch(_json, actionPatch);

            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedAction);

            // Assert
            Assert.AreEqual(PersonResponsible.Adviser, action.PersonResponsible);
        }

        [Test]
        public void ActionPatchServiceTests_CheckLastModifiedDateIsUpdated_WhenPatchIsCalled()
        {
            var actionPatch = new ActionPatch {LastModifiedDate = DateTime.MaxValue};

            var patchedAction = _actionPatchService.Patch(_json, actionPatch);

            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedAction);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, action.LastModifiedDate);
        }

        [Test]
        public void ActionPatchServiceTests_CheckLastModifiedTouchpointIdIsUpdated_WhenPatchIsCalled()
        {
            var actionPatch = new ActionPatch {LastModifiedTouchpointId = "0000000111"};

            var patchedAction = _actionPatchService.Patch(_json, actionPatch);

            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedAction);

            // Assert
            Assert.AreEqual("0000000111", action.LastModifiedTouchpointId);
        }
    }
}
