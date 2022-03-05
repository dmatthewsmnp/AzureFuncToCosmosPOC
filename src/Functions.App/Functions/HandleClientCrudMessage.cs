using System;
using System.Collections.Generic;
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
                var clientCrudMessage = CrudMessageEnvelopeOfT<MpmClientCrudPayload>.Deserialize(myQueueItem);
                if (clientCrudMessage.Payload != null)
                {
                    _logger.LogInformation("Upserting Client {ClientId} for [{EventID}:{Operation}:{EventDateTime}]",
                        clientCrudMessage.Payload.ClientId, clientCrudMessage.EventId, clientCrudMessage.Operation, clientCrudMessage.EventDatetime);
                    var response = await _cosmosDbUtils.GetContainer("Client").UpsertItemAsync(
                        new Client(
                            id: clientCrudMessage.Payload.ClientId,
                            ClientNo: clientCrudMessage.Payload.ClientNo,
                            ClientName: clientCrudMessage.Payload.ClientName,
                            BusinessPhone: clientCrudMessage.Payload.BusinessPhone,
                            BusinessFax: clientCrudMessage.Payload.BusinessFax,
                            Website: clientCrudMessage.Payload.Website,
                            Street1: clientCrudMessage.Payload.Street1,
                            Street2: clientCrudMessage.Payload.Street2,
                            Street3: clientCrudMessage.Payload.Street3,
                            City: clientCrudMessage.Payload.City,
                            Province: clientCrudMessage.Payload.Province,
                            PostalCode: clientCrudMessage.Payload.PostalCode,
                            Country: clientCrudMessage.Payload.Country
                        ),
                        requestOptions: _itemRequestOptions);
                    _logger.LogInformation("UpsertItemAsync<Client> result {StatusCode}", response.StatusCode);
                }
            }
            catch (DeserializationException de)
            {
                _logger.LogError(de, "Error deserializing payload");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request");
                throw;
            }
        }
    }
}
