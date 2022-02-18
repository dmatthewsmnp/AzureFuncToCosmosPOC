using System.Collections.Generic;
using Functions.Domain.Models;

namespace Functions.Domain.Responses;

/// <summary>
/// Strongly-typed version of TypedResponse (help OpenAPI generation)
/// </summary>
public class PersonResponse : TypedResponse<IEnumerable<Person>>
{
	public PersonResponse(IEnumerable<Person> payload, string? traceId) : base(payload, traceId)
	{
	}
}
