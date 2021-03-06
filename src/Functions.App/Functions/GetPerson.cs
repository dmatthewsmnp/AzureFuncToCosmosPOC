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

public class GetPerson
{
	#region Fields and constructor
	private readonly ILogger _logger;
	private readonly CosmosDbUtils _cosmosDbUtils;
	public GetPerson(ILoggerFactory loggerFactory, CosmosDbUtils cosmosDbUtils)
	{
		_logger = loggerFactory.CreateLogger<GetPerson>();
		_cosmosDbUtils = cosmosDbUtils;
	}
	#endregion

	[Function("GetPerson")]
	[OpenApiOperation(operationId: "GetPerson", tags: new[] { "person" })]
	[OpenApiParameter(name: "id", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Type = typeof(Guid), Required = true)]
	[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PersonResponse), Description = "OK response")]
	[OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(BadRequestResponse), Description = "Invalid request")]
	[OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(BasicResponse), Description = "Invalid person or person not found")]
	[OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(BasicResponse), Description = "Error")]
	public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "persons/{id:guid}")] HttpRequestData req, Guid id)
	{
		using var logscope = _logger.BeginScope(new Dictionary<string, object?>()
		{
			{ "TraceID", req.FunctionContext.InvocationId },
			{ "Person.id", id }
		});
		try
		{
			_logger.LogDebug("Retrieving Person");
			var response = await _cosmosDbUtils.GetContainer("Person").ReadItemAsync<Person>(id.ToString(), new PartitionKey(id.ToString()));
			_logger.LogDebug("ReadItemAsync<Person> result {StatusCode}", response.StatusCode);
			return await ResponseFactory.Create<PersonResponse, IEnumerable<Person>>(req, new List<Person>() { response.Resource }, HttpStatusCode.OK);
		}
		catch (CosmosException ce) when (ce.StatusCode == HttpStatusCode.NotFound)
		{
			return await ResponseFactory.Create(req, HttpStatusCode.NotFound);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving Person");
			return await ResponseFactory.ServerError(req);
		}
	}
}
