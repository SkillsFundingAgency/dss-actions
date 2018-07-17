﻿using System;
using System.Threading.Tasks;

namespace NCS.DSS.Action.GetActionByIdHttpTrigger.Service
{
    public interface IGetActionByIdHttpTriggerService
    {
        Task<Models.Action> GetActionForCustomerAsync(Guid customerId, Guid actionId);
    }
}