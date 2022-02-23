using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Functions.App.Utilities;
using Functions.Domain.Exceptions;
using Functions.Domain.Models;
using Functions.Domain.Responses;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace Functions.App.Functions;

public class PutPerson
{
	#region Fields and constructor
	private static readonly ItemRequestOptions _itemRequestOptions = new() { EnableContentResponseOnWrite = false };
	private readonly ILogger _logger;
	private readonly CosmosDbUtils _cosmosDbUtils;
	public PutPerson(ILoggerFactory loggerFactory, CosmosDbUtils cosmosDbUtils)
	{
		_logger = loggerFactory.CreateLogger<PutPerson>();
		_cosmosDbUtils = cosmosDbUtils;
	}
	#endregion

	/// <summary>
	/// Logic for PUT method on persons endpoint
	/// </summary>
	[Function("PutPerson")]
	[OpenApiOperation(operationId: "PutPerson", tags: new[] { "person" })]
	[OpenApiParameter(name: "id", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Type = typeof(Guid), Required = true)]
	[OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Person))]
	[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PersonResponse), Description = "OK response")]
	[OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(BadRequestResponse), Description = "Invalid request")]
	[OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(BasicResponse), Description = "Error")]
	public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "persons/{id:guid}")] HttpRequestData req, Guid id)
	{
		using var logscope = _logger.BeginScope(new Dictionary<string, object?>()
		{
			{ "TraceID", req.FunctionContext.InvocationId },
			{ "Person.id", id }
		});
		try
		{
			// Deserialize request body into Person model and assign ID:
			var person = await RequestFactory.DeserializeBody<Person>(req);
			person.id = id;

			// Perform upsert of object in container:
			_logger.LogDebug("Upserting Person");
			var response = await _cosmosDbUtils.GetContainer("Person").UpsertItemAsync(person, requestOptions: _itemRequestOptions);
			_logger.LogInformation("UpsertItemAsync<Person> result {StatusCode}", response.StatusCode);
			return await ResponseFactory.Create<PersonResponse, IEnumerable<Person>>(req, new List<Person>() { person },
				// Use response status code in the specific case of HTTP 201, otherwise just use HTTP 200 rather than leaking:
				response.StatusCode == HttpStatusCode.Created ? HttpStatusCode.Created : HttpStatusCode.OK);
		}
		catch (ModelValidationException ve)
		{
			_logger.LogDebug("Request validation failed {@InvalidFields}", ve.InvalidFields);
			return await ResponseFactory.BadRequest(req, ve.InvalidFields);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error processing request");
			return await ResponseFactory.ServerError(req);
		}
	}
}