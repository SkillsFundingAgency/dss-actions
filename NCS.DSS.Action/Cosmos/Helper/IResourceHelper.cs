using System;

namespace NCS.DSS.Action.Cosmos.Helper
{
    public interface IResourceHelper
    {
        bool DoesCustomerExist(Guid customerId);
        bool DoesInteractionExist(Guid interactionId);
        bool DoesActionPlanExist(Guid actionPlanId);
    }
}