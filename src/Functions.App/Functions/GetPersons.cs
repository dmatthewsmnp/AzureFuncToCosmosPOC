using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Functions.App.Utilities;
using Functions.Domain.Models;
using Functions.Domain.Responses;
using Functions.Domain.Utilities;
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
    [OpenApiParameter(name: "favColour", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Type = typeof(Colour), Required = false)]
    [OpenApiParameter(name: "PageLimit", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Type = typeof(int), Required = false)]
    [OpenApiParameter(name: "PageOffset", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Type = typeof(int), Required = false)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PersonResponse), Description = "OK response")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(BadRequestResponse), Description = "Invalid request")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(BasicResponse), Description = "Error")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "persons")] HttpRequestData req,
        string? favColour = null, int? PageLimit = null, int? PageOffset = null)
    {
        var favColourEnum = EnumFlexConverter<Colour>.GetFromObject(favColour);
        _logger.LogInformation("Received GET request [{favColour}:{PageLimit}:{PageOffset}]", favColourEnum, PageLimit, PageOffset);

        // TODO: Fetch results

        return await ResponseFactory.Create<PersonResponse, IEnumerable<Person>>(req, new List<Person>()
        {
            new()
            {
                id = Guid.NewGuid(),
                firstName = "First",
                lastName = "Last",
                favColour = favColourEnum
            }
        }, HttpStatusCode.OK);
    }
}
