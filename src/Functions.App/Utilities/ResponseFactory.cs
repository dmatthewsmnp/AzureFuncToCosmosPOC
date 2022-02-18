using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Functions.Domain.Models;
using Functions.Domain.Responses;
using Microsoft.Azure.Functions.Worker.Http;

namespace Functions.App.Utilities;

/// <summary>
/// Utility class providing response object helper methods
/// </summary>
public static class ResponseFactory
{
	/// <summary>
	/// Create an HttpResponseData object with default BasicResponse model
	/// </summary>
	public static async Task<HttpResponseData> Create(HttpRequestData req, HttpStatusCode httpStatusCode)
	{
		var response = req.CreateResponse();
		await response.WriteAsJsonAsync(new BasicResponse(req.FunctionContext.InvocationId), httpStatusCode);
		return response;
	}

	/// <summary>
	/// Create an HttpResponseData object with TypedResponse model using provided payload
	/// </summary>
	/// <remarks>
	/// Two types and constraint here are to allow for strongly-typed response classes (for purposes of OpenAPI generation),
	/// while ensuring that those types are always the correct TypedResponses; consider whether this is necessary
	/// </remarks>
	public static async Task<HttpResponseData> Create<TResp, TPayload>(HttpRequestData req, TPayload payload, HttpStatusCode httpStatusCode) where TResp : TypedResponse<TPayload>
	{
		var response = req.CreateResponse();
		await response.WriteAsJsonAsync(new TypedResponse<TPayload>(payload, req.FunctionContext.InvocationId), httpStatusCode);
		return response;
	}

	/// <summary>
	/// Create HTTP 400 Bad Request response with TypedResponse model containing invalid field collection payload
	/// </summary>
	public static async Task<HttpResponseData> BadRequest(HttpRequestData req, IEnumerable<InvalidField> invalidFields)
	{
		var response = req.CreateResponse();
		await response.WriteAsJsonAsync(new BadRequestResponse(invalidFields, req.FunctionContext.InvocationId), HttpStatusCode.BadRequest);
		return response;
	}

	/// <summary>
	/// Create HTTP 500 Internal Server Error response with default BasicResponse model
	/// </summary>
	public static async Task<HttpResponseData> ServerError(HttpRequestData req)
	{
		var response = req.CreateResponse();
		await response.WriteAsJsonAsync(new BasicResponse(req.FunctionContext.InvocationId), HttpStatusCode.InternalServerError);
		return response;
	}
}
