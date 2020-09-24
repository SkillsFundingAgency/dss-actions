using System;
using DFC.JSON.Standard;
using NCS.DSS.Action.Models;
using NCS.DSS.Action.PatchActionHttpTrigger.Service;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;

namespace NCS.DSS.Action.Tests.ServiceTests
{

    public class ActionPatchServiceTests
    {
        private readonly IJsonHelper _jsonHelper;
        private readonly IActionPatchService _actionPatchService;
        private readonly ActionPatch _actionPatch;
        private readonly string _json;

        public ActionPatchServiceTests()
        {
            _jsonHelper = Substitute.For<JsonHelper>();
            _actionPatchService = Substitute.For<ActionPatchService>(_jsonHelper);
            _actionPatch = Substitute.For<ActionPatch>();

            _json = JsonConvert.SerializeObject(_actionPatch);
        }

        [Fact]
        public void ActionPatchServiceTests_ReturnsNull_WhenActionPatchIsNull()
        {
            var result = _actionPatchService.Patch(string.Empty, _actionPatch);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ActionPatchServiceTests_CheckDateActionAgreedIsUpdated_WhenPatchIsCalled()
        {
            var actionPatch = new ActionPatch() { DateActionAgreed = DateTime.MaxValue };

            var patchedActionPlan = _actionPatchService.Patch(_json, actionPatch);

            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedActionPlan);

            // Assert
            Assert.Equal(DateTime.MaxValue, action.DateActionAgreed);
        }


        [Fact]
        public void ActionPatchServiceTests_CheckDateActionAimsToBeCompletedByIsUpdated_WhenPatchIsCalled()
        {
            var actionPatch = new ActionPatch { DateActionAimsToBeCompletedBy = DateTime.MaxValue };

            var patchedActionPlan = _actionPatchService.Patch(_json, actionPatch);

            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedActionPlan);

            // Assert
            Assert.Equal(DateTime.MaxValue, action.DateActionAimsToBeCompletedBy);
        }

        [Fact]
        public void ActionPatchServiceTests_CheckDateActionActuallyCompletedIsUpdated_WhenPatchIsCalled()
        {
            var actionPatch = new ActionPatch { DateActionActuallyCompleted = DateTime.MaxValue };

            var patchedActionPlan = _actionPatchService.Patch(_json, actionPatch);

            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedActionPlan);

            // Assert
            Assert.Equal(DateTime.MaxValue, action.DateActionActuallyCompleted);
        }

        [Fact]
        public void ActionPatchServiceTests_CheckActionSummaryIsUpdated_WhenPatchIsCalled()
        {
            var actionPatch = new ActionPatch { ActionSummary = "TestActionSummary" };

            var patchedActionPlan = _actionPatchService.Patch(_json, actionPatch);

            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedActionPlan);

            // Assert
            Assert.Equal("TestActionSummary", action.ActionSummary);
        }

        [Fact]
        public void ActionPatchServiceTests_CheckSignpostedToMethodIsUpdated_WhenPatchIsCalled()
        {
            var actionPatch = new ActionPatch { SignpostedTo = "SignpostedToTest" };

            var patchedActionPlan = _actionPatchService.Patch(_json, actionPatch);

            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedActionPlan);

            // Assert
            Assert.Equal("SignpostedToTest", action.SignpostedTo);
        }

        [Fact]
        public void ActionPatchServiceTests_CheckSignpostedCategoryIsUpdated_WhenPatchIsCalled()
        {
            var actionPatch = new ActionPatch { SignpostedToCategory = DSS.Action.ReferenceData.SignpostedToCategory.Other };

            var patchedActionPlan = _actionPatchService.Patch(_json, actionPatch);

            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedActionPlan);

            // Assert
            Assert.Equal(DSS.Action.ReferenceData.SignpostedToCategory.Other, action.SignpostedToCategory);
        }

        [Fact]
        public void ActionPatchServiceTests_CheckActionTypeIsUpdated_WhenPatchIsCalled()
        {
            var actionPatch = new ActionPatch { ActionType = DSS.Action.ReferenceData.ActionType.ApplyForApprenticeship };

            var patchedActionPlan = _actionPatchService.Patch(_json, actionPatch);

            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedActionPlan);

            // Assert
            Assert.Equal(DSS.Action.ReferenceData.ActionType.ApplyForApprenticeship, action.ActionType);
        }

        [Fact]
        public void ActionPatchServiceTests_CheckActionStatusIsUpdated_WhenPatchIsCalled()
        {
            var actionPatch = new ActionPatch { ActionStatus = DSS.Action.ReferenceData.ActionStatus.InProgress };

            var patchedActionPlan = _actionPatchService.Patch(_json, actionPatch);

            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedActionPlan);

            // Assert
            Assert.Equal(DSS.Action.ReferenceData.ActionStatus.InProgress, action.ActionStatus);
        }

        [Fact]
        public void ActionPatchServiceTests_CheckPersonResponsibleIsUpdated_WhenPatchIsCalled()
        {
            var actionPatch = new ActionPatch { PersonResponsible = DSS.Action.ReferenceData.PersonResponsible.Adviser };

            var patchedActionPlan = _actionPatchService.Patch(_json, actionPatch);

            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedActionPlan);

            // Assert
            Assert.Equal(DSS.Action.ReferenceData.PersonResponsible.Adviser, action.PersonResponsible);
        }

        [Fact]
        public void ActionPatchServiceTests_CheckLastModifiedDateIsUpdated_WhenPatchIsCalled()
        {
            var actionPatch = new ActionPatch { LastModifiedDate = DateTime.MaxValue };

            var patchedActionPlan = _actionPatchService.Patch(_json, actionPatch);

            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedActionPlan);

            // Assert
            Assert.Equal(DateTime.MaxValue, action.LastModifiedDate);
        }

        [Fact]
        public void ActionPatchServiceTests_CheckLastModifiedTouchpointIdIsUpdated_WhenPatchIsCalled()
        {
            var actionPatch = new ActionPatch { LastModifiedTouchpointId = "0000000111" };

            var patchedActionPlan = _actionPatchService.Patch(_json, actionPatch);

            var action = JsonConvert.DeserializeObject<Action.Models.Action>(patchedActionPlan);

            // Assert
            Assert.Equal("0000000111", action.LastModifiedTouchpointId);
        }

    }
}
