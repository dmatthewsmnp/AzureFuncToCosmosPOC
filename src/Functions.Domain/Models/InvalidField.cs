using System.Text.Json.Serialization;

namespace Functions.Domain.Models;

/// <summary>
/// Container for details of failed model/field validation
/// </summary>
public class InvalidField
{
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? field { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? message { get; set; }

	public InvalidField(string? field, string? message)
	{
		this.field = field;
		this.message = message;
	}
}