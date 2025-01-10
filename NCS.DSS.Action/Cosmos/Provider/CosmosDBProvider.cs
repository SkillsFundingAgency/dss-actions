using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Action.Models;

namespace NCS.DSS.Action.Cosmos.Provider
{
    public class CosmosDBProvider : ICosmosDBProvider
    {
        private readonly Container _actionContainer;
        private readonly Container _actionPlanContainer;
        private readonly Container _interactionContainer;
        private readonly Container _customerContainer;
        private readonly PartitionKey _partitionKey = PartitionKey.None;
        private readonly ILogger<CosmosDBProvider> _logger;

        public CosmosDBProvider(
            CosmosClient cosmosClient, 
            IOptions<ActionConfigurationSettings> configOptions,
            ILogger<CosmosDBProvider> logger)
        {
            var config = configOptions.Value;

            _actionContainer = GetContainer(cosmosClient, config.DatabaseId, config.CollectionId);
            _customerContainer = GetContainer(cosmosClient, config.CustomerDatabaseId, config.CustomerCollectionId);
            _actionPlanContainer = GetContainer(cosmosClient, config.ActionPlanDatabaseId, config.ActionPlanCollectionId);
            _interactionContainer = GetContainer(cosmosClient, config.InteractionDatabaseId, config.InteractionCollectionId);
            _logger = logger;
        }

        private static Container GetContainer(CosmosClient cosmosClient, string databaseId, string collectionId) 
            => cosmosClient.GetContainer(databaseId, collectionId);

        public async Task<bool> DoesCustomerResourceExistAsync(Guid customerId)
        {
            _logger.LogInformation("Checking if customer resource exists for Customer ID: {CustomerId}", customerId);

            try
            {
                var response = await _customerContainer.ReadItemAsync<Customer>(customerId.ToString(), _partitionKey);
                _logger.LogInformation("Customer resource found for Customer ID: {CustomerId}", customerId);

                return response.Resource != null;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Customer resource not found for Customer ID: {CustomerId}", customerId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking customer resource existence");
                throw;
            }
        }

        public async Task<bool> DoesCustomerHaveATerminationDateAsync(Guid customerId)
        {
            _logger.LogInformation("Checking for termination date. Customer ID: {CustomerId}", customerId);

            try
            {
                var response = await _customerContainer.ReadItemAsync<Customer>(
                    customerId.ToString(),
                    _partitionKey);

                var dateOfTermination = response.Resource?.DateOfTermination;
                var hasTerminationDate = dateOfTermination != null;

                _logger.LogInformation("Termination date check completed. CustomerId: {CustomerId}. HasTerminationDate: {HasTerminationDate}", customerId, hasTerminationDate);
                return hasTerminationDate;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // If a 404 occurs, the resource does not exist
                _logger.LogInformation("Customer does not exist. Customer ID: {CustomerId}", customerId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking termination date. Customer ID: {CustomerId}", customerId);
                throw;
            }
        }

        //TODO: Need to test
        public async Task<bool> DoesInteractionResourceExistAndBelongToCustomerAsync(Guid interactionId, Guid customerId)
        {
            _logger.LogInformation("Checking if interaction resource exists and belongs to Customer ID: {CustomerId}, Interaction ID: {InteractionId}", customerId, interactionId);

            try
            {
                var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM interactions i WHERE i.id = @interactionId AND i.CustomerId = @customerId")
                    .WithParameter("@interactionId", interactionId)
                    .WithParameter("@customerId", customerId);

                var iterator = _interactionContainer.GetItemQueryIterator<long>(query, requestOptions: new QueryRequestOptions
                {
                    PartitionKey = _partitionKey
                });

                var count = (await iterator.ReadNextAsync()).Resource.FirstOrDefault();
                var exists = count > 0;
                _logger.LogInformation("Interaction resource existence check result: {Result}", exists);

                return exists;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error checking interaction resource existence");
                return false;
            }
        }

        //TODO: Need to test
        public async Task<bool> DoesActionPlanResourceExistAndBelongToCustomerAsync(Guid actionPlanId, Guid interactionId, Guid customerId)
        {
            _logger.LogInformation("Checking if action plan resource exists for Action Plan ID: {ActionPlanId}, Interaction ID: {InteractionId}, Customer ID: {CustomerId}", actionPlanId, interactionId, customerId);

            try
            {
                var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM actionplans a WHERE a.id = @actionPlanId AND a.InteractionId = @interactionId AND a.CustomerId = @customerId")
                    .WithParameter("@actionPlanId", actionPlanId)
                    .WithParameter("@interactionId", interactionId)
                    .WithParameter("@customerId", customerId);

                var iterator = _actionPlanContainer.GetItemQueryIterator<long>(query, requestOptions: new QueryRequestOptions
                {
                    PartitionKey = _partitionKey
                });

                var count = (await iterator.ReadNextAsync()).Resource.FirstOrDefault();
                var exists = count > 0;

                _logger.LogInformation("Action plan resource existence check result: {Result}", exists);

                return exists;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error checking action plan resource existence");
                return false;
            }
        }

        public async Task<List<Models.Action>> GetActionsForCustomerAsync(Guid customerId, Guid actionPlanId)
        {
            _logger.LogInformation("Retrieving actions for Customer ID: {CustomerId}, Action Plan ID: {ActionPlanId}", customerId, actionPlanId);

            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.CustomerId = @customerId AND c.ActionPlanId = @actionPlanId")
                    .WithParameter("@customerId", customerId)
                    .WithParameter("@actionPlanId", actionPlanId);

                var actions = new List<Models.Action>();
                var iterator = _actionContainer.GetItemQueryIterator<Models.Action>(query, requestOptions: new QueryRequestOptions
                {
                    PartitionKey = _partitionKey
                });

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    actions.AddRange(response.Resource);
                }

                _logger.LogInformation("Retrieved {Count} actions for CustomerId: {CustomerId}", actions.Count, customerId);

                return actions.Any() ? actions : null;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error fetching actions");
                return null;
            }
        }

        public async Task<Models.Action> GetActionForCustomerAsync(Guid customerId, Guid actionId, Guid actionPlanId)
        {
            _logger.LogInformation("Retrieving action for Customer ID: {CustomerId}. Action ID: {ActionId}. Action Plan ID: {ActionPlanId}", customerId, actionId, actionPlanId);

            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.CustomerId = @customerId AND c.ActionId = @actionId AND c.ActionPlanId = @actionPlanId")
                    .WithParameter("@customerId", customerId)
                    .WithParameter("@actionId", actionId)
                    .WithParameter("@actionPlanId", actionPlanId);

                var iterator = _actionContainer.GetItemQueryIterator<Models.Action>(query, requestOptions: new QueryRequestOptions
                {
                    PartitionKey = _partitionKey
                });

                var response = await iterator.ReadNextAsync();

                return response.Resource.FirstOrDefault();
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error retrieving action for customer");
                return null;
            }
        }

        public async Task<string> GetActionForCustomerToUpdateAsync(Guid customerId, Guid actionId, Guid actionPlanId)
        {
            _logger.LogInformation("Retrieving action for update for Customer ID: {CustomerId}. Action ID: {ActionId}. Action Plan ID: {ActionPlanId}", customerId, actionId, actionPlanId);

            try
            {
                var action = await GetActionForCustomerAsync(customerId, actionId, actionPlanId);
                return action?.ToString();
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error retrieving action for update");
                return null;
            }
        }

        public async Task<ItemResponse<Models.Action>> CreateActionAsync(Models.Action action)
        {
            _logger.LogInformation("Creating action for Customer ID: {CustomerId}", action.CustomerId);

            try
            {
                var response = await _actionContainer.CreateItemAsync(action, _partitionKey);
                _logger.LogInformation("Action created successfully for Customer ID: {CustomerId}", action.CustomerId);

                return response;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error creating action");
                return null;
            }
        }

        public async Task<ItemResponse<Models.Action>> UpdateActionAsync(Models.Action action, Guid actionId)
        {
            _logger.LogInformation("Updating action for Action ID: {ActionId}", actionId);

            try
            {
                var response = await _actionContainer.ReplaceItemAsync(action, actionId.ToString(), PartitionKey.None);
                _logger.LogInformation("Action updated successfully for Action ID: {ActionId}", actionId);

                return response;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error updating action");
                return null;
            }
        }

        public async Task<ItemResponse<Models.Action>> UpdateActionAsync(string actionJson, Guid actionId)
        {
            if (string.IsNullOrWhiteSpace(actionJson))
            {
                _logger.LogWarning("Empty or null action data provided for Action ID: {ActionId}", actionId);
                return null;
            }

            try
            {
                _logger.LogInformation("Updating action for Action ID: {ActionId}", actionId);

                var actionDocument = JsonSerializer.Deserialize<Models.Action>(actionJson);

                var response = await _actionContainer.ReplaceItemAsync(actionDocument, actionId.ToString(), _partitionKey);

                _logger.LogInformation("Action updated successfully for Action ID: {ActionId}", actionId);

                return response;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing Action JSON for Action ID: {ActionId}", actionId);
                return null;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error updating Action for Action ID: {ActionId}", actionId);
                return null;
            }
        }


    }
}