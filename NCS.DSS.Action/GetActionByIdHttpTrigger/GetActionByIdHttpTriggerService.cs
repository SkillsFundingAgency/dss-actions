﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NCS.DSS.Action.GetActionByIdHttpTrigger
{
    public class GetActionByIdHttpTriggerService
    {
        public async Task<Models.Action> GetAction(Guid actionId)
        {
            var actions = CreateTempActions();
            var result = actions.FirstOrDefault(a => a.ActionId == actionId);
            return await Task.FromResult(result);
        }

        public List<Models.Action> CreateTempActions()
        {
            var actionList = new List<Models.Action>
            {
                new Models.Action
                {
                    ActionId = Guid.Parse("489cc04f-399f-41cb-9afe-1934884f3c5f"),
                    CustomerId = Guid.NewGuid(),
                    ActionPlanId = Guid.NewGuid(),
                    DateActionAgreed = DateTime.Today.AddDays(-5),
                    DateActionAimsToBeCompletedBy = DateTime.Today.AddDays(10),
                    DateActionActuallyCompleted = DateTime.Today.AddDays(12),
                    ActionSummary = "This is a fake summary",
                    ActionTypeId = 1,
                    ActionStatusId = 1,
                    PersonResponsibleId = 1,
                    LastModifiedDate = DateTime.Today.AddYears(1),
                    LastModifiedBy = Guid.NewGuid()
                },
                new Models.Action
                {
                    ActionId = Guid.Parse("4221d30e-1d56-42dd-bae9-2f20e519b261"),
                    CustomerId = Guid.NewGuid(),
                    ActionPlanId = Guid.NewGuid(),
                    DateActionAgreed = DateTime.Today,
                    DateActionAimsToBeCompletedBy = DateTime.Today.AddDays(5),
                    DateActionActuallyCompleted = DateTime.Today.AddDays(5),
                    ActionSummary = "This is a fake summary v2",
                    ActionTypeId = 2,
                    ActionStatusId = 2,
                    PersonResponsibleId = 2,
                    LastModifiedDate = DateTime.Today.AddYears(1),
                    LastModifiedBy = Guid.NewGuid()
                },
                new Models.Action
                {
                    ActionId = Guid.Parse("bc5ac80d-f820-4cd8-8505-548c9c9db5a5"),
                    CustomerId = Guid.NewGuid(),
                    ActionPlanId = Guid.NewGuid(),
                    DateActionAgreed = DateTime.Today.AddDays(-20),
                    DateActionAimsToBeCompletedBy = DateTime.Today.AddDays(2),
                    DateActionActuallyCompleted = DateTime.Today.AddDays(1),
                    ActionSummary = "This is a fake summary v3",
                    ActionTypeId = 3,
                    ActionStatusId = 3,
                    PersonResponsibleId = 3,
                    LastModifiedDate = DateTime.Today,
                    LastModifiedBy = Guid.NewGuid()
                }
            };

            return actionList;
        }
    }
}