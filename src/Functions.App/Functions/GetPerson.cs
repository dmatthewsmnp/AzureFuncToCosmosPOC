using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Functions.App.Utilities;
using Functions.Domain.Models;
using Functions.Domain.Responses;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace Functions.App.Functions;

public class GetPerson
{
	#region Fields and constructor
	private readonly ILogger _logger;
    public GetPerson(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<GetPerson>();
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
        _logger.LogInformation("Received GET request for {id}", id);
        // TODO: Fetch result
        return await ResponseFactory.OK<PersonResponse, IEnumerable<Person>>(req, new List<Person>()
        {
            new()
            {
                id = id,
                firstName = "First",
                lastName = "Last",
                favColour = Colour.Green
            }
        });
    }
}
