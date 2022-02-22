using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Functions.Domain.Models;

public class Person
{
	/// <summary>
	/// Unique ID of this person
	/// </summary>
	/// <remarks>
	/// Consider whether this should be included in *request* model, or only response model (and part of route for PUT/PATCH?)
	/// </remarks>
	[Required]
	public Guid? id { get; init; } = null;

	[Required]
	[StringLength(maximumLength: 50, MinimumLength = 2)]
	public string? firstName { get; init; } = null;

	[Required]
	[StringLength(maximumLength: 50, MinimumLength = 2)]
	public string? lastName { get; init; } = null;

	public const string REGEX_COLOUR = "^[A-Za-z]+$";

	[RegularExpression(REGEX_COLOUR)]
	public string? favColour { get; init; } = null;
}
