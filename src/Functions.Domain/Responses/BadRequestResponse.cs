using System.Collections.Generic;
using Functions.Domain.Models;

namespace Functions.Domain.Responses;

/// <summary>
/// Strongly-typed version of TypedResponse (help OpenAPI generation)
/// </summary>
public class BadRequestResponse : TypedResponse<IEnumerable<InvalidField>>
{
	public BadRequestResponse(IEnumerable<InvalidField> payload, string? traceId) : base(payload, traceId)
	{
	}
}
