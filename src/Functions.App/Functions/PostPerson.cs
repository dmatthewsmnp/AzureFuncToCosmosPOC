using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Functions.App.Utilities;
using Functions.Domain.Exceptions;
using Functions.Domain.Models;
using Functions.Domain.Responses;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace Functions.App.Functions;

public class PostPerson
{
	#region Fields and constructor
	private readonly ILogger _logger;
	private readonly CosmosDbUtils _cosmosDbUtils;
	public PostPerson(ILoggerFactory loggerFactory, CosmosDbUtils cosmosDbUtils)
	{
		_logger = loggerFactory.CreateLogger<PostPerson>();
		_cosmosDbUtils = cosmosDbUtils;
	}
	#endregion

	/// <summary>
	/// Logic for POST/PUT method on persons endpoint
	/// </summary>
	/// <remarks>
	/// To consider: in theory PUT (and/or PATCH) should have {id} in URI, in this case separate it from POST (and remove id property from request model?)
	/// </remarks>
	[Function("PostPerson")]
	[OpenApiOperation(operationId: "PostPerson", tags: new[] { "person" })]
	[OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Person))]
	[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PersonResponse), Description = "OK response")]
	[OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(BadRequestResponse), Description = "Invalid request")]
	[OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(BasicResponse), Description = "Error")]
	public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", "put", Route = "persons")] HttpRequestData req)
	{
		try
		{
			// Deserialize request body into Person model:
			var person = await RequestFactory.DeserializeBody<Person>(req);
			_logger.LogDebug("Received Person {id}", person.id);

			// Perform upsert of object in container:
			var response = await _cosmosDbUtils.GetContainer("Person").UpsertItemAsync(person);
			if ((int)response.StatusCode >= 200 && (int)response.StatusCode <= 299 && response.Resource != null)
			{
				_logger.LogInformation("Person {id} {HttpMethod} DB result {StatusCode}", person.id, req.Method, response.StatusCode);
				return await ResponseFactory.Create<PersonResponse, IEnumerable<Person>>(req, new List<Person>() { response.Resource }, response.StatusCode);
				// Note: using Resource in payload rather than model means metadata added by Cosmos is included - desired or no?
			}
			else
			{
				_logger.LogWarning("Person {id} {HttpMethod} DB result {StatusCode}", person.id, req.Method, response.StatusCode);
				return await ResponseFactory.ServerError(req);
			}
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
