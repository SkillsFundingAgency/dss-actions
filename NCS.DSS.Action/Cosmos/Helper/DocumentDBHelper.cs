
using System;
using System.Configuration;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Action.Cosmos.Helper
{
    public static class DocumentDBHelper
    {
        private static Uri _documentCollectionUri;
        private static Uri _documentUri;
        private static readonly string DatabaseId = ConfigurationManager.AppSettings["DatabaseId"];
        private static readonly string CollectionId = ConfigurationManager.AppSettings["CollectionId"];

        private static Uri _customerDocumentCollectionUri;
        private static readonly string CustomerDatabaseId = ConfigurationManager.AppSettings["CustomerDatabaseId"];
        private static readonly string CustomerCollectionId = ConfigurationManager.AppSettings["CustomerCollectionId"];

        private static Uri _interactionDocumentCollectionUri;
        private static readonly string InteractionDatabaseId = ConfigurationManager.AppSettings["InteractionDatabaseId"];
        private static readonly string InteractionCollectionId = ConfigurationManager.AppSettings["InteractionCollectionId"];

        private static Uri _actionPlanDocumentCollectionUri;
        private static readonly string ActionPlanDatabaseId = ConfigurationManager.AppSettings["ActionPlanDatabaseId"];
        private static readonly string ActionPlanCollectionId = ConfigurationManager.AppSettings["ActionPlanCollectionId"];

        public static Uri CreateDocumentCollectionUri()
        {
            if (_documentCollectionUri != null)
                return _documentCollectionUri;

            _documentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                DatabaseId,
                CollectionId);

            return _documentCollectionUri;
        }
        
        public static Uri CreateDocumentUri(Guid actionPlanId)
        {
            if (_documentUri != null)
                return _documentUri;

            _documentUri = UriFactory.CreateDocumentUri(DatabaseId, CollectionId, actionPlanId.ToString());

            return _documentUri;

        }

        #region CustomerDB

        public static Uri CreateCustomerDocumentCollectionUri()
        {
            if (_customerDocumentCollectionUri != null)
                return _customerDocumentCollectionUri;

            _customerDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                CustomerDatabaseId, CustomerCollectionId);

            return _customerDocumentCollectionUri;
        }

        #endregion

        #region InteractionDB

        public static Uri CreateInteractionDocumentCollectionUri()
        {
            if (_interactionDocumentCollectionUri != null)
                return _interactionDocumentCollectionUri;

            _interactionDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                InteractionDatabaseId, InteractionCollectionId);

            return _interactionDocumentCollectionUri;
        }

        #endregion

        #region ActionPlanDB

        public static Uri CreateActionPlanDocumentCollectionUri()
        {
            if (_actionPlanDocumentCollectionUri != null)
                return _actionPlanDocumentCollectionUri;

            _actionPlanDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                ActionPlanDatabaseId, ActionPlanCollectionId);

            return _actionPlanDocumentCollectionUri;
        }

        #endregion   

    }
}
