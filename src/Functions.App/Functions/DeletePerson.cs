using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Functions.App.Utilities;
using Functions.Domain.Models;
using Functions.Domain.Responses;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace Functions.App.Functions;

public class DeletePerson
{
	#region Fields and constructor
	private readonly ILogger _logger;
	private readonly CosmosDbUtils _cosmosDbUtils;
	public DeletePerson(ILoggerFactory loggerFactory, CosmosDbUtils cosmosDbUtils)
	{
		_logger = loggerFactory.CreateLogger<DeletePerson>();
		_cosmosDbUtils = cosmosDbUtils;
	}
	#endregion

	[Function("DeletePerson")]
	[OpenApiOperation(operationId: "DeletePerson", tags: new[] { "person" })]
	[OpenApiParameter(name: "id", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Type = typeof(Guid), Required = true)]
	[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(BasicResponse), Description = "OK response")]
	[OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(BadRequestResponse), Description = "Invalid request")]
	[OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(BasicResponse), Description = "Invalid person or person not found")]
	[OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(BasicResponse), Description = "Error")]
	public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "persons/{id:guid}")] HttpRequestData req, Guid id)
	{
		using var logscope = _logger.BeginScope(new Dictionary<string, object?>()
		{
			{ "TraceID", req.FunctionContext.InvocationId },
			{ "Person.id", id }
		});
		try
		{
			// Perform delete of object in container:
			_logger.LogDebug("Deleting Person");
			var response = await _cosmosDbUtils.GetContainer("Person").DeleteItemAsync<Person>(id.ToString(), new PartitionKey(id.ToString()));
			_logger.LogInformation("DeleteItemAsync<Person> result {StatusCode}", response.StatusCode);
			return await ResponseFactory.Create(req, HttpStatusCode.OK); // Typically would be HTTP 204, but we want to include response content
		}
		catch (CosmosException ce) when (ce.StatusCode == HttpStatusCode.NotFound)
		{
			_logger.LogInformation("Person not found");
			return await ResponseFactory.Create(req, HttpStatusCode.NotFound);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting Person");
			return await ResponseFactory.ServerError(req);
		}
	}
}
