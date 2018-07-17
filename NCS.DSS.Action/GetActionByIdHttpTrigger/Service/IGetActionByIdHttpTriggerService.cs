using System;
using System.Threading.Tasks;

namespace NCS.DSS.Action.GetActionByIdHttpTrigger.Service
{
    public interface IGetActionByIdHttpTriggerService
    {
        Task<Models.Action> GetActionPlanForCustomerAsync(Guid customerId, Guid actionId);
    }
}