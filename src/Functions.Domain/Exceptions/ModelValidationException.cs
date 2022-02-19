using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using Functions.Domain.Models;

namespace Functions.Domain.Exceptions;

/// <summary>
/// Custom exception class for emitting errors out of model validation
/// </summary>
[ExcludeFromCodeCoverage]
[Serializable] // Satisfy code analyzer
public class ModelValidationException : Exception
{
	public IEnumerable<InvalidField> InvalidFields { get; }

	/// <summary>
	/// Public constructor - initialize InvalidFields list from single ValidationResult
	/// </summary>
	public ModelValidationException(ValidationResult validationResult) : base()
	{
		InvalidFields = new List<InvalidField>() { new(validationResult.MemberNames.FirstOrDefault(), validationResult.ErrorMessage) };
	}
	/// <summary>
	/// Public constructor - initialize InvalidFields list from ValidationResult list
	/// </summary>
	public ModelValidationException(List<ValidationResult> validationResults) : base()
	{
		InvalidFields = validationResults.SelectMany(x => x.MemberNames.DefaultIfEmpty(), (vresult, fieldname) => new InvalidField(fieldname, vresult.ErrorMessage));
	}

	/// <summary>
	/// Protected serialization constructor (to satisfy code analyzer)
	/// </summary>
	protected ModelValidationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		InvalidFields = new List<InvalidField>();
	}
}
