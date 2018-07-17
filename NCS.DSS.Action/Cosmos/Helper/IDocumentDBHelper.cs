using System;

namespace NCS.DSS.Action.Cosmos.Helper
{
    public interface IDocumentDBHelper
    {
        Uri CreateDocumentCollectionUri();
        Uri CreateDocumentUri(Guid actionPlanId);
        Uri CreateCustomerDocumentCollectionUri();
        Uri CreateInteractionDocumentCollectionUri();
    }
}