﻿using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using NCS.DSS.Action.Cosmos.Client;
using NCS.DSS.Action.Cosmos.Helper;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.Action.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {


        private string _customerJson;

        public string GetCustomerJson()
        {
            return _customerJson;
        }

        public async Task<bool> DoesCustomerResourceExist(Guid customerId)
        {
            var documentUri = DocumentDBHelper.CreateCustomerDocumentUri(customerId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;
            try
            {
                var response = await client.ReadDocumentAsync(documentUri);
                if (response.Resource != null)
                {
                    _customerJson = response.Resource.ToString();
                    return true;
                }
            }
            catch (DocumentClientException)
            {
                return false;
            }

            return false;
        }

        public bool DoesInteractionResourceExistAndBelongToCustomer(Guid interactionId, Guid customerId)
        {
            var collectionUri = DocumentDBHelper.CreateInteractionDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;

            try
            {
                var query = client.CreateDocumentQuery<long>(collectionUri, new SqlQuerySpec()
                {
                    QueryText = "SELECT VALUE COUNT(1) FROM interactions i " +
                                "WHERE i.id = @interactionId " +
                                "AND i.CustomerId = @customerId",

                    Parameters = new SqlParameterCollection()
                    {
                        new SqlParameter("@interactionId", interactionId),
                        new SqlParameter("@customerId", customerId)
                    }
                }).AsEnumerable().FirstOrDefault();

                return Convert.ToBoolean(Convert.ToInt16(query));
            }
            catch (DocumentQueryException)
            {
                return false;
            }

        }

        public bool DoesActionPlanResourceExistAndBelongToCustomer(Guid actionPlanId, Guid interactionId, Guid customerId)
        {
            var collectionUri = DocumentDBHelper.CreateActionPlanDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;

            try
            {
                var query = client.CreateDocumentQuery<long>(collectionUri, new SqlQuerySpec()
                {
                    QueryText = "SELECT VALUE COUNT(1) FROM actionplans a " +
                                "WHERE a.id = @actionPlanId " +
                                "AND a.InteractionId = @interactionId " +
                                "AND a.CustomerId = @customerId",

                    Parameters = new SqlParameterCollection()
                    {
                        new SqlParameter("@actionPlanId", actionPlanId),
                        new SqlParameter("@interactionId", interactionId),
                        new SqlParameter("@customerId", customerId)
                    }
                }).AsEnumerable().FirstOrDefault();

                return Convert.ToBoolean(Convert.ToInt16(query));
            }
            catch (DocumentQueryException)
            {
                return false;
            }

        }

        public async Task<List<Models.Action>> GetActionsForCustomerAsync(Guid customerId, Guid actionPlanId)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();
            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var actionsQuery = client.CreateDocumentQuery<Models.Action>(collectionUri)
                .Where(so => so.CustomerId == customerId && so.ActionPlanId == actionPlanId).AsDocumentQuery();

            var actions = new List<Models.Action>();

            while (actionsQuery.HasMoreResults)
            {
                var response = await actionsQuery.ExecuteNextAsync<Models.Action>();
                actions.AddRange(response);
            }

            return actions.Any() ? actions : null;
        }

        public async Task<Models.Action> GetActionForCustomerAsync(Guid customerId, Guid actionId, Guid actionPlanId)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            var actionForCustomerQuery = client
                ?.CreateDocumentQuery<Models.Action>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId &&
                            x.ActionId == actionId &&
                            x.ActionPlanId == actionPlanId)
                .AsDocumentQuery();

            if (actionForCustomerQuery == null)
                return null;

            var actions = await actionForCustomerQuery.ExecuteNextAsync<Models.Action>();

            return actions?.FirstOrDefault();
        }

        public async Task<string> GetActionForCustomerToUpdateAsync(Guid customerId, Guid actionId, Guid actionPlanId)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            var actionForCustomerQuery = client
                ?.CreateDocumentQuery<Models.Action>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId && x.ActionId == actionId && x.ActionPlanId == actionPlanId)
                .AsDocumentQuery();

            if (actionForCustomerQuery == null)
                return null;

            var actions = await actionForCustomerQuery.ExecuteNextAsync();

            return actions?.FirstOrDefault()?.ToString();
        }

        public async Task<ResourceResponse<Document>> CreateActionAsync(Models.Action action)
        {

            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, action);

            return response;

        }

        public async Task<ResourceResponse<Document>> UpdateActionAsync(string action, Guid actionId)
        {
            if (string.IsNullOrEmpty(action))
                return null;

            var documentUri = DocumentDBHelper.CreateDocumentUri(actionId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var actionDocumentJObject = JObject.Parse(action);

            var response = await client.ReplaceDocumentAsync(documentUri, actionDocumentJObject);

            return response;
        }

    }
}