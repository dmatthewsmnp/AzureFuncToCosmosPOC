using System;
using System.Collections.Generic;
using System.Linq;
using Functions.Domain.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Functions.App.Functions
{
    public class HandleClientChangeFeed
    {
		#region Fields and constructor
		private readonly ILogger _logger;
        public HandleClientChangeFeed(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HandleClientChangeFeed>();
        }
        #endregion

        [Function("HandleClientChangeFeed")]
        [ServiceBusOutput("dih/client/v1", Connection = "AzureWebJobsServiceBus", EntityType = EntityType.Topic)]
        public string[] Run([CosmosDBTrigger(
            databaseName: "%DBName%",
            containerName: "Client",
            Connection = "CosmosDB",
            LeaseContainerName = "ClientLeases")] IReadOnlyList<Client> input)
        {
            if (input != null && input.Count > 0)
            {
                _logger.LogInformation("Documents modified: " + input.Count);
                return input.Select(client => client.id.ToString()).ToArray();
            }
            else
            {
                return Array.Empty<string>();
            }
        }
    }
}
