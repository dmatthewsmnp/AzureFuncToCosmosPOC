using System;
using System.Linq;
using Newtonsoft.Json;

namespace Functions.Domain.Utilities;

/// <summary>
/// Custom converter class for Enum values
/// </summary>
/// <remarks>Uses Newtonsoft.Json rather than System.Text.Json as Azure Functions does not yet support the latter?</remarks>
public class EnumFlexConverter<T> : JsonConverter<T?> where T : struct
{
	/// <summary>
	/// Convert value from case-insensitive string, integer or integer string value
	/// </summary>
	public static T? GetFromObject(object? value)
	{
		// Using Enum.IsDefined prevents case-insensitive search, using Enum.TryParse lets invalid integers through; do
		// direct case-insensitive comparison of string values instead:
		if (value is string valueStr && Enum.GetNames(typeof(T)).Any(x => x.Equals(valueStr, StringComparison.OrdinalIgnoreCase)))
		{
			// Enum.TryParse seems unnecessary since we know this is a valid value - but Enum.Parse is case-sensitive:
			return (T?)(Enum.TryParse(typeof(T), valueStr, true, out var result) ? result : null);
		}
		// If integer value was provided, may be possible to just cast directly - but integer type chosen by deserializer
		// can be unpredictable (maybe int, maybe long, etc) so convert to string and back:
		else if (int.TryParse(value?.ToString(), out var valueInt) && Enum.IsDefined(typeof(T), valueInt))
		{
			return (T?)(Enum.TryParse(typeof(T), value!.ToString(), true, out var result) ? result : null);
		}
		return null;
	}

	/// <summary>
	/// Serialize value to Json as enum string value
	/// </summary>
	public override void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer)
	{
		writer.WriteValue(value?.ToString());
	}

	/// <summary>
	/// Deserialize value from Json as case-insensitive string, integer or integer string value
	/// </summary>
	/// <remarks>
	/// Will default to existingValue, if incoming value cannot be deserialized (so value in model
	/// object will be left at whatever default is defined for that model class)
	/// </remarks>
	public override T? ReadJson(JsonReader reader, Type objectType, T? existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		return GetFromObject(reader.Value) ?? existingValue;
	}
}