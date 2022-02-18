namespace Functions.Domain.Responses;

/// <summary>
/// Generic model for a basic API response with typed payload added
/// </summary>
public class TypedResponse<T> : BasicResponse
{
	public T payload { get; set; }

	public TypedResponse(T payload, string? traceId)
		: base(traceId)
	{
		this.payload = payload;
	}
}
