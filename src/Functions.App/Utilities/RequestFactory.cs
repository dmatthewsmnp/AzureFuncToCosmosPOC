using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Functions.Domain.Exceptions;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

namespace Functions.App.Utilities;

/// <summary>
/// Utility class providing request object helper methods
/// </summary>
public static class RequestFactory
{
	/// <summary>
	/// Read request payload, attempt deserialization into an object of class T and perform model validation
	/// </summary>
	/// <exception cref="ModelValidationException">Thrown if invalid data received or model validation fails</exception>
	public static async Task<T> DeserializeBody<T>(HttpRequestData req) where T : class
	{
		var requestBody = await req.ReadAsStringAsync();
		try
		{
			var t = string.IsNullOrEmpty(requestBody) ? null : JsonConvert.DeserializeObject<T>(requestBody);
			if (t == null)
			{
				throw new ModelValidationException(new List<ValidationResult>() { new("A non-empty request body is required") });
			}

			// Attempt validation - return deserialized object if successful, otherwise generate exception:
			var validationResults = new List<ValidationResult>();
			return Validator.TryValidateObject(t, new ValidationContext(t, null, null), validationResults, true) ?
				t : throw new ModelValidationException(validationResults);
		}
		catch (JsonException)
		{
			throw new ModelValidationException(new List<ValidationResult>() { new("Invalid request body") });
		}
	}
}
