using System;
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
    private readonly CosmosClient _cosmosClient;
    public DeletePerson(ILoggerFactory loggerFactory, CosmosClient cosmosClient)
    {
        _logger = loggerFactory.CreateLogger<DeletePerson>();
        _cosmosClient = cosmosClient;
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
        _logger.LogDebug("Received DELETE request for {id}", id);

        try
        {
            // Perform delete of object in container:
            var container = _cosmosClient.GetContainer("dm-poc-data", "Person"); // TODO: Make DB name configurable?
            var response = await container.DeleteItemAsync<Person>(id.ToString(), new PartitionKey(id.ToString()));
            if ((int)response.StatusCode >= 200 && (int)response.StatusCode <= 299)
            {
                _logger.LogInformation("Person {id} {HttpMethod} DB result {StatusCode}", id, req.Method, response.StatusCode);
                return await ResponseFactory.Create(req, response.StatusCode);
            }
            else
            {
                _logger.LogWarning("Person {id} {HttpMethod} DB result {StatusCode}", id, req.Method, response.StatusCode);
                return await ResponseFactory.Create(req, response.StatusCode); // TODO: Replace (or map) status code? Use standard value for DB decline?
            }
        }
        catch (CosmosException ce)
        {
            _logger.LogWarning(ce, "Database declined request");
            return await ResponseFactory.Create(req, ce.StatusCode); // TODO: Replace (or map) status code?
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Person");
            return await ResponseFactory.ServerError(req);
        }
    }
}
