using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Functions.App.Utilities;
using Functions.Domain.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MNP.ServiceFabric.Shared.Envelope.V1;

namespace Functions.App.Functions
{
    public class HandleClientCrudMessage
    {
		#region Fields and constructors
		private static readonly ItemRequestOptions _itemRequestOptions = new() { EnableContentResponseOnWrite = false };
        private readonly ILogger _logger;
        private readonly CosmosDbUtils _cosmosDbUtils;
        public HandleClientCrudMessage(ILoggerFactory loggerFactory, CosmosDbUtils cosmosDbUtils)
        {
            _logger = loggerFactory.CreateLogger<HandleClientCrudMessage>();
            _cosmosDbUtils = cosmosDbUtils;
        }
        #endregion

        [Function("HandleClientCrudMessage")]
        public async Task Run(
            [ServiceBusTrigger(topicName: "%ServiceBusTopic%", subscriptionName: "%ServiceBusSubscription%")] string myQueueItem,
            int deliveryCount,
            DateTime enqueuedTimeUtc,
            string messageId)
        {
            using var logscope = _logger.BeginScope(new Dictionary<string, object?>() { { "TraceID", messageId } });
            try
            {
                var clientCrudMessage = CrudMessageEnvelope<MpmClientCrudPayload>.Deserialize(myQueueItem)
                    ?? throw new InvalidOperationException("No MpmClientCrudPayload received");

                try
                {
                    _logger.LogInformation("Upserting Client {ClientId} for [{EventID}:{Operation}:{EventDateTime}]",
                        clientCrudMessage.Payload.ClientId, clientCrudMessage.EventId, clientCrudMessage.Operation, clientCrudMessage.EventDatetime);

                    var patchOperations = new List<PatchOperation>
                    {
                        PatchOperation.Set("MPM_Client_No", clientCrudMessage.Payload.ClientNo),
                        PatchOperation.Set("ClientName", clientCrudMessage.Payload.ClientName),
                        PatchOperation.Set("BusinessPhone", clientCrudMessage.Payload.BusinessPhone),
                        PatchOperation.Set("BusinessFax", clientCrudMessage.Payload.BusinessFax),
                        PatchOperation.Set("Website", clientCrudMessage.Payload.Website),
                        PatchOperation.Set("Street1", clientCrudMessage.Payload.Street1),
                        PatchOperation.Set("Street2", clientCrudMessage.Payload.Street2),
                        PatchOperation.Set("City", clientCrudMessage.Payload.City),
                        PatchOperation.Set("Province", clientCrudMessage.Payload.Province),
                        PatchOperation.Set("PostalCode", clientCrudMessage.Payload.PostalCode),
                        PatchOperation.Set("Country", clientCrudMessage.Payload.Country),
                        PatchOperation.Set("LastUpdatedEventId", clientCrudMessage.EventId),
                        PatchOperation.Set("LastUpdatedEventEnqueued", enqueuedTimeUtc),
                        PatchOperation.Increment("Version", 1)
                    };
                    var response = await _cosmosDbUtils.GetContainer("Client").PatchItemAsync<Client>(
                        id: clientCrudMessage.Payload.ClientId.ToString(),
                        partitionKey: new PartitionKey(clientCrudMessage.Payload.ClientId.ToString()),
                        patchOperations: patchOperations);
                    _logger.LogInformation("PatchItemAsync<Client> result {StatusCode}", response.StatusCode);
                }
                catch (CosmosException ce) when (ce.StatusCode == HttpStatusCode.NotFound)
                {
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request");
                throw;
            }
        }
    }
}
