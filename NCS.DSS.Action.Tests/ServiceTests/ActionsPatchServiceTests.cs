using DFC.JSON.Standard;
using NCS.DSS.Action.Models;
using NCS.DSS.Action.PatchActionHttpTrigger.Service;
using Newtonsoft.Json;
using NUnit.Framework;
using System;

namespace NCS.DSS.Action.Tests.ServiceTests
{
    [TestFixture]
    public class ActionPatchServiceTests
    {
        private IJsonHelper _jsonHelper;
        private IActionPatchService _actionPatchService;
        private ActionPatch _actionPatch;
        private readonly string _json;

        public ActionPatchServiceTests()
        {
            _jsonHelper = new JsonHelper();
            _actionPatchService = new ActionPatchService(_jsonHelper);
            _actionPatch = new ActionPatch();
            _json = JsonConvert.SerializeObject(_actionPatch);
        }

        [Test]
        public void ActionPatchServiceTests_ReturnsNull_WhenActionPatchIsNull()
        {
            // Act
            var result = _actionPatchService.Patch(string.Empty, _actionPatch);

            // Assert
            Assert.Null(result);
        }

        [Test]
        public void ActionPatchServiceTests_CheckDateActionAgreedIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var actionPatch = new ActionPatch() { DateActionAgreed = DateTime.MaxValue };

            // Act
            var patchedActionPlan = _actionPatchService.Patch(_json, actionPatch);

            // Assert
            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedActionPlan);
            Assert.AreEqual(DateTime.MaxValue, action.DateActionAgreed);
        }


        [Test]
        public void ActionPatchServiceTests_CheckDateActionAimsToBeCompletedByIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var actionPatch = new ActionPatch { DateActionAimsToBeCompletedBy = DateTime.MaxValue };

            // Act
            var patchedActionPlan = _actionPatchService.Patch(_json, actionPatch);

            // Assert
            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedActionPlan);
            Assert.AreEqual(DateTime.MaxValue, action.DateActionAimsToBeCompletedBy);
        }

        [Test]
        public void ActionPatchServiceTests_CheckDateActionActuallyCompletedIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var actionPatch = new ActionPatch { DateActionActuallyCompleted = DateTime.MaxValue };

            // Act
            var patchedActionPlan = _actionPatchService.Patch(_json, actionPatch);

            // Assert
            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedActionPlan);
            Assert.AreEqual(DateTime.MaxValue, action.DateActionActuallyCompleted);
        }

        [Test]
        public void ActionPatchServiceTests_CheckActionSummaryIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var actionPatch = new ActionPatch { ActionSummary = "TestActionSummary" };

            // Act
            var patchedActionPlan = _actionPatchService.Patch(_json, actionPatch);

            // Assert
            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedActionPlan);
            Assert.AreEqual("TestActionSummary", action.ActionSummary);
        }

        [Test]
        public void ActionPatchServiceTests_CheckSignpostedToMethodIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var actionPatch = new ActionPatch { SignpostedTo = "SignpostedToTest" };

            // Act
            var patchedActionPlan = _actionPatchService.Patch(_json, actionPatch);

            // Assert
            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedActionPlan);
            Assert.AreEqual("SignpostedToTest", action.SignpostedTo);
        }

        [Test]
        public void ActionPatchServiceTests_CheckSignpostedCategoryIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var actionPatch = new ActionPatch { SignpostedToCategory = DSS.Action.ReferenceData.SignpostedToCategory.Other };

            // Act
            var patchedActionPlan = _actionPatchService.Patch(_json, actionPatch);

            // Assert
            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedActionPlan);
            Assert.AreEqual(DSS.Action.ReferenceData.SignpostedToCategory.Other, action.SignpostedToCategory);
        }

        [Test]
        public void ActionPatchServiceTests_CheckActionTypeIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var actionPatch = new ActionPatch { ActionType = DSS.Action.ReferenceData.ActionType.ApplyForApprenticeship };

            // Act
            var patchedActionPlan = _actionPatchService.Patch(_json, actionPatch);

            // Assert
            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedActionPlan);
            Assert.AreEqual(DSS.Action.ReferenceData.ActionType.ApplyForApprenticeship, action.ActionType);
        }

        [Test]
        public void ActionPatchServiceTests_CheckActionStatusIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var actionPatch = new ActionPatch { ActionStatus = DSS.Action.ReferenceData.ActionStatus.InProgress };

            // Act
            var patchedActionPlan = _actionPatchService.Patch(_json, actionPatch);

            // Assert
            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedActionPlan);
            Assert.AreEqual(DSS.Action.ReferenceData.ActionStatus.InProgress, action.ActionStatus);
        }

        [Test]
        public void ActionPatchServiceTests_CheckPersonResponsibleIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var actionPatch = new ActionPatch { PersonResponsible = DSS.Action.ReferenceData.PersonResponsible.Adviser };

            // Act
            var patchedActionPlan = _actionPatchService.Patch(_json, actionPatch);

            // Assert
            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedActionPlan);
            Assert.AreEqual(DSS.Action.ReferenceData.PersonResponsible.Adviser, action.PersonResponsible);
        }

        [Test]
        public void ActionPatchServiceTests_CheckLastModifiedDateIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var actionPatch = new ActionPatch { LastModifiedDate = DateTime.MaxValue };

            // Act
            var patchedActionPlan = _actionPatchService.Patch(_json, actionPatch);

            // Assert
            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedActionPlan);
            Assert.AreEqual(DateTime.MaxValue, action.LastModifiedDate);
        }

        [Test]
        public void ActionPatchServiceTests_CheckLastModifiedTouchpointIdIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var actionPatch = new ActionPatch { LastModifiedTouchpointId = "0000000111" };

            // Act
            var patchedActionPlan = _actionPatchService.Patch(_json, actionPatch);

            // Assert
            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedActionPlan);
            Assert.AreEqual("0000000111", action.LastModifiedTouchpointId);
        }

    }
}
