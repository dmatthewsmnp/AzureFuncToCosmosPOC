using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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

public class GetPersons
{
	#region Fields and constructor
	private readonly ILogger _logger;
	private readonly CosmosClient _cosmosClient;
	private static readonly Regex _regexPersonColour = new(Person.REGEX_COLOUR);
	public GetPersons(ILoggerFactory loggerFactory, CosmosClient cosmosClient)
	{
		_logger = loggerFactory.CreateLogger<GetPersons>();
		_cosmosClient = cosmosClient;
	}
	#endregion

	[Function("GetPersons")]
	[OpenApiOperation(operationId: "GetPersons", tags: new[] { "person" })]
	[OpenApiParameter(name: "favColour", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Type = typeof(string), Required = false, Description = $"Must match regex: {Person.REGEX_COLOUR}")]
	[OpenApiParameter(name: "PageLimit", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Type = typeof(int), Required = false, Description = "Maximum number of rows to return")]
	[OpenApiParameter(name: "PageOffset", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Type = typeof(int), Required = false, Description = "Starting offset (ignored if PageLimit not provided)")]
	[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PersonResponse), Description = "OK response")]
	[OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(BadRequestResponse), Description = "Invalid request")]
	[OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(BasicResponse), Description = "Error")]
	public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "persons")] HttpRequestData req,
		string? favColour = null, int? PageLimit = null, int? PageOffset = null)
	{
		_logger.LogDebug("Received GET request [{favColour}:{PageLimit}:{PageOffset}]", favColour, PageLimit, PageOffset);

		try
		{
			#region Build SQL query definition
			var queryBuilder = new StringBuilder("SELECT * FROM Person p");
			var queryParms = new Dictionary<string, object>();
			if (!string.IsNullOrEmpty(favColour))
			{
				// Since favColour is free text string, make sure it matches validation regex before using:
				if (!_regexPersonColour.IsMatch(favColour))
				{
					throw new ModelValidationException(new ValidationResult("Invalid format", new[] { nameof(favColour) }));
				}
				queryBuilder.Append($" WHERE p.favColour = @favColour");
				queryParms["@favColour"] = favColour;
			}
			if (PageLimit > 0) // Ignore PageOffset if PageLimit not provided
			{
				queryBuilder.Append($" OFFSET @PageOffset LIMIT @PageLimit");
				queryParms["@PageOffset"] = PageOffset ?? 0;
				queryParms["@PageLimit"] = PageLimit;
			}

			// Construct query definition object and apply parameters (if any):
			var query = new QueryDefinition(queryBuilder.ToString());
			foreach (var kvp in queryParms)
			{
				query.WithParameter(kvp.Key, kvp.Value);
			}
			#endregion

			// Open container and execute query:
			var container = _cosmosClient.GetContainer("dm-poc-data", "Person"); // TODO: Make DB name configurable?
			var queryIterator = container.GetItemQueryIterator<Person>(query);
			var personList = new List<Person>();
			while (queryIterator.HasMoreResults)
			{
				// Add all entries in range to collection:
				personList.AddRange(await queryIterator.ReadNextAsync());
			}
			return await ResponseFactory.Create<PersonResponse, IEnumerable<Person>>(req, personList, HttpStatusCode.NotImplemented);
		}
		catch (ModelValidationException ve)
		{
			_logger.LogDebug("Request validation failed {@InvalidFields}", ve.InvalidFields);
			return await ResponseFactory.BadRequest(req, ve.InvalidFields);
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
