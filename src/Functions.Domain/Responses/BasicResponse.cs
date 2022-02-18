using System;

namespace Functions.Domain.Responses;

/// <summary>
/// Model for basic API response without detail payload
/// </summary>
public class BasicResponse
{
	public string serverTimestamp { get; }

	public string? traceId { get; }

	public BasicResponse(string? traceId)
	{
		serverTimestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
		this.traceId = traceId;
	}
}
