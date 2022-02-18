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
    private readonly CosmosClient _cosmosClient;
    public GetPerson(ILoggerFactory loggerFactory, CosmosClient cosmosClient)
    {
        _logger = loggerFactory.CreateLogger<GetPerson>();
        _cosmosClient = cosmosClient;
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
        _logger.LogDebug("Received GET request for {id}", id);

        try
        {
            var container = _cosmosClient.GetContainer("dm-poc-data", "Person"); // TODO: Make DB name configurable?
            var response = await container.ReadItemAsync<Person>(id.ToString(), new PartitionKey(id.ToString()));
            if ((int)response.StatusCode >= 200 && (int)response.StatusCode <= 299 && response.Resource != null)
            {
                return await ResponseFactory.Create<PersonResponse, IEnumerable<Person>>(req, new List<Person>() { response.Resource }, HttpStatusCode.OK);
                // Note: using Resource in payload rather than model means metadata added by Cosmos is included - desired or no?
            }
            else
            {
                return await ResponseFactory.Create(req, response.StatusCode); // TODO: Replace (or map) status code?
            }
        }
        catch (CosmosException ce)
        {
            _logger.LogWarning(ce, "Database declined request");
            return await ResponseFactory.Create(req, ce.StatusCode); // TODO: Replace (or map) status code?
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Person");
            return await ResponseFactory.ServerError(req);
        }
    }
}
