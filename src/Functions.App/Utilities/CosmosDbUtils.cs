using System;
using Azure.Identity;
using Microsoft.Azure.Cosmos;

namespace Functions.App.Utilities;

public class CosmosDbUtils
{
	private readonly CosmosClient _cosmosClient;
	private readonly string _dbName;

	/// <summary>
	/// Public constructor
	/// </summary>
	/// <param name="connectionString">Full connection string (will be used, if provided)</param>
	/// <param name="endpoint">Endpoint address only, if using managed identity</param>
	/// <param name="dbName">Name of database to be used by this function app</param>
	/// <exception cref="ArgumentNullException"></exception>
	public CosmosDbUtils(string? connectionString, string? endpoint, string? dbName)
	{
		if (!string.IsNullOrEmpty(connectionString))
		{
			_cosmosClient = new(connectionString);
		}
		else if (!string.IsNullOrEmpty(endpoint))
		{
			_cosmosClient = new(endpoint, new ManagedIdentityCredential());
		}
		else
		{
			throw new ArgumentException("Either connection string or endpoint address is required");
		}
		_dbName = dbName ?? throw new ArgumentNullException(nameof(dbName));
	}

	/// <summary>
	/// Retrieve the specified container from CosmosDB connection
	/// </summary>
	public Container GetContainer(string containerName)
	{
		return _cosmosClient.GetContainer(_dbName, containerName);
	}
}
