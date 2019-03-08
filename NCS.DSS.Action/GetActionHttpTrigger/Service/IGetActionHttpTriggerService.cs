using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.Action.GetActionHttpTrigger.Service
{
    public interface IGetActionHttpTriggerService
    {
        Task<List<Models.Action>> GetActionsAsync(Guid customerId, Guid actionPlanId);
    }
}