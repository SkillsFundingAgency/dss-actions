using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using NCS.DSS.Action.Cosmos.Client;
using NCS.DSS.Action.Cosmos.Helper;

namespace NCS.DSS.Action.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {
        private readonly DocumentDBHelper _documentDbHelper;
        private readonly DocumentDBClient _databaseClient;

        public DocumentDBProvider()
        {
            _documentDbHelper = new DocumentDBHelper();
            _databaseClient = new DocumentDBClient();
        }

        public bool DoesCustomerResourceExist(Guid customerId)
        {
            var collectionUri = _documentDbHelper.CreateCustomerDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return false;

            var customerQuery = client.CreateDocumentQuery<Document>(collectionUri, new FeedOptions() { MaxItemCount = 1 });
            return customerQuery.Where(x => x.Id == customerId.ToString()).Select(x => x.Id).AsEnumerable().Any();
        }

        public async Task<bool> DoesCustomerHaveATerminationDate(Guid customerId)
        {
            var collectionUri = _documentDbHelper.CreateCustomerDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            var customerByIdQuery = client
                ?.CreateDocumentQuery<Document>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.Id == customerId.ToString())
                .AsDocumentQuery();

            if (customerByIdQuery == null)
                return false;

            var customerQuery = await customerByIdQuery.ExecuteNextAsync<Document>();

            var customer = customerQuery?.FirstOrDefault();

            if (customer == null)
                return false;

            var dateOfTermination = customer.GetPropertyValue<DateTime?>("DateOfTermination");

            return dateOfTermination.HasValue;
        }

        public bool DoesInteractionResourceExist(Guid interactionId)
        {
            var collectionUri = _documentDbHelper.CreateInteractionDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return false;

            var interactionQuery = client.CreateDocumentQuery<Document>(collectionUri, new FeedOptions() { MaxItemCount = 1 });
            return interactionQuery.Where(x => x.Id == interactionId.ToString()).Select(x => x.Id).AsEnumerable().Any();
        }

        public bool DoesActionPlanResourceExist(Guid actionPlanId)
        {
            var collectionUri = _documentDbHelper.CreateActionPlanDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return false;

            var actionPlanQuery = client.CreateDocumentQuery<Document>(collectionUri, new FeedOptions() { MaxItemCount = 1 });
            return actionPlanQuery.Where(x => x.Id == actionPlanId.ToString()).Select(x => x.Id).AsEnumerable().Any();
        }

        public async Task<List<Models.Action>> GetActionsForCustomerAsync(Guid customerId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var actionsQuery = client.CreateDocumentQuery<Models.Action>(collectionUri)
                .Where(so => so.CustomerId == customerId).AsDocumentQuery();

            var actions = new List<Models.Action>();

            while (actionsQuery.HasMoreResults)
            {
                var response = await actionsQuery.ExecuteNextAsync<Models.Action>();
                actions.AddRange(response);
            }

            return actions.Any() ? actions : null;
        }

        public async Task<Models.Action> GetActionForCustomerAsync(Guid customerId, Guid actionId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            var actionForCustomerQuery = client
                ?.CreateDocumentQuery<Models.Action>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId && x.ActionId == actionId)
                .AsDocumentQuery();

            if (actionForCustomerQuery == null)
                return null;

            var actions = await actionForCustomerQuery.ExecuteNextAsync<Models.Action>();

            return actions?.FirstOrDefault();
        }

        public async Task<ResourceResponse<Document>> CreateActionAsync(Models.Action action)
        {

            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, action);

            return response;

        }

        public async Task<ResourceResponse<Document>> UpdateActionAsync(Models.Action action)
        {
            var documentUri = _documentDbHelper.CreateDocumentUri(action.ActionId.GetValueOrDefault());

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.ReplaceDocumentAsync(documentUri, action);

            return response;
        }
    }
}