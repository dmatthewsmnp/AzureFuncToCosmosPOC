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

public class GetPersons
{
	#region Fields and constructor
	private readonly ILogger _logger;
	private readonly CosmosClient _cosmosClient;
	public GetPersons(ILoggerFactory loggerFactory, CosmosClient cosmosClient)
	{
		_logger = loggerFactory.CreateLogger<GetPersons>();
		_cosmosClient = cosmosClient;
	}
	#endregion

	[Function("GetPersons")]
	[OpenApiOperation(operationId: "GetPersons", tags: new[] { "person" })]
	[OpenApiParameter(name: "favColour", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Type = typeof(string), Required = false)]
	[OpenApiParameter(name: "PageLimit", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Type = typeof(int), Required = false)]
	[OpenApiParameter(name: "PageOffset", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Type = typeof(int), Required = false)]
	[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PersonResponse), Description = "OK response")]
	[OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(BadRequestResponse), Description = "Invalid request")]
	[OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(BasicResponse), Description = "Error")]
	public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "persons")] HttpRequestData req,
		string? favColour = null, int? PageLimit = null, int? PageOffset = null)
	{
		_logger.LogDebug("Received GET request [{favColour}:{PageLimit}:{PageOffset}]", favColour, PageLimit, PageOffset);

		try
		{
			var container = _cosmosClient.GetContainer("dm-poc-data", "Person"); // TODO: Make DB name configurable?
			// TODO: Fetch results

			return await ResponseFactory.Create(req, HttpStatusCode.NotImplemented);
		}
		catch (CosmosException ce)
		{
			_logger.LogWarning(ce, "Database declined request");
			return await ResponseFactory.Create(req, ce.StatusCode); // TODO: Replace (or map) status code?
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving Persons");
			return await ResponseFactory.ServerError(req);
		}
	}
}
