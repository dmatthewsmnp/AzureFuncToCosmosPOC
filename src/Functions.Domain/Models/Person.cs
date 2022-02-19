using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

	/// <summary>
	/// Any unrecognized/unexpected fields will be parsed into extension collection (allows for
	/// this model definition to be out of date with other processes feeding backing datastore)
	/// </summary>
	[JsonExtensionData]
	public Dictionary<string, JToken>? extensionData { get; set; }
}
