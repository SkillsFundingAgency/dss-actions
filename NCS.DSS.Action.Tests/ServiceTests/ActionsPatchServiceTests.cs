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
            Assert.That(result, Is.Null);
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
            Assert.That(DateTime.MaxValue, Is.EqualTo(action.DateActionAgreed));
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
            Assert.That(DateTime.MaxValue, Is.EqualTo(action.DateActionAimsToBeCompletedBy));
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
            Assert.That(DateTime.MaxValue, Is.EqualTo(action.DateActionActuallyCompleted));
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
            Assert.That("TestActionSummary", Is.EqualTo(action.ActionSummary));
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
            Assert.That("SignpostedToTest", Is.EqualTo(action.SignpostedTo));
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
            Assert.That(DSS.Action.ReferenceData.SignpostedToCategory.Other, Is.EqualTo(action.SignpostedToCategory));
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
            Assert.That(DSS.Action.ReferenceData.ActionType.ApplyForApprenticeship, Is.EqualTo(action.ActionType));
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
            Assert.That(DSS.Action.ReferenceData.ActionStatus.InProgress, Is.EqualTo(action.ActionStatus));
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
            Assert.That(DSS.Action.ReferenceData.PersonResponsible.Adviser, Is.EqualTo(action.PersonResponsible));
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
            Assert.That(DateTime.MaxValue, Is.EqualTo(action.LastModifiedDate));
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
            Assert.That("0000000111", Is.EqualTo(action.LastModifiedTouchpointId));
        }

    }
}
