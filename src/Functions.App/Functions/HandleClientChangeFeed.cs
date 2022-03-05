using System;
using System.Collections.Generic;
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
        public void Run([CosmosDBTrigger(
            databaseName: "fx-poc-db",
            collectionName: "Client",
            ConnectionStringSetting = "CosmosDBConnection",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)] IReadOnlyList<Client> input)
        {
            if (input != null && input.Count > 0)
            {
                _logger.LogInformation("Documents modified: " + input.Count);
                _logger.LogInformation("First document Id: " + input[0].id);
            }
        }
    }
}
