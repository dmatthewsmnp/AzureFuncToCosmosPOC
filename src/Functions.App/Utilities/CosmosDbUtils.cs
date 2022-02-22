using System;
using Microsoft.Azure.Cosmos;

namespace Functions.App.Utilities;

public class CosmosDbUtils
{
	private readonly CosmosClient _cosmosClient;
	private readonly string _dbName;
	public CosmosDbUtils(string? connectionString, string? dbName)
	{
		_cosmosClient = new(connectionString ?? throw new ArgumentNullException(nameof(connectionString)));
		_dbName = dbName ?? throw new ArgumentNullException(nameof(dbName));
	}

	public Container GetContainer(string containerName)
	{
		return _cosmosClient.GetContainer(_dbName, containerName);
	}
}
