using System;
using System.Net;
using System.Threading.Tasks;
using Functions.App.Utilities;
using Functions.Domain.Responses;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace Functions.App.Functions;

public class DeletePerson
{
	#region Fields and constructor
	private readonly ILogger _logger;
    public DeletePerson(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<GetPerson>();
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
        _logger.LogInformation("Received DELETE request for {id}", id);
        // TODO: Execute delete
        return await ResponseFactory.OK(req);
    }
}
