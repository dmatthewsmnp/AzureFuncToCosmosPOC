using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Functions.Domain.Models;

public class Person : IValidatableObject
{
	/// <summary>
	/// Unique ID of this person (response-only field)
	/// </summary>
	/// <remarks>
	/// Note that value must be NULL on incoming POST requests (value will be auto-generated
	/// by function) and PUT requests (value must be specified via route), but will be present
	/// in successful GET/POST/PUT responses
	/// </remarks>
	public Guid? id { get; set; } = null;

	[Required]
	[StringLength(maximumLength: 50, MinimumLength = 2)]
	public string? firstName { get; init; } = null;

	[Required]
	[StringLength(maximumLength: 50, MinimumLength = 2)]
	public string? lastName { get; init; } = null;

	public const string REGEX_COLOUR = "^[A-Za-z]+$";

	[RegularExpression(REGEX_COLOUR)]
	public string? favColour { get; init; } = null;

	/// <summary>
	/// Custom validation method - ensure "id" property is not set on request
	/// </summary>
	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		if (id != null)
		{
			yield return new ValidationResult("Request value must be null", new[] { nameof(id) });
		}
	}
}
