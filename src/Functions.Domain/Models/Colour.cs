using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Functions.Domain.Models;

/// <summary>
/// Enumeration for Colour - specifies StringEnumConverter so that Swaggerfile will
/// be generated with string values, but actual models will use EnumFlexConverter
/// </summary>
/// <remarks>Uses Newtonsoft.Json rather than System.Text.Json as Azure Functions does not yet support the latter?</remarks>
[JsonConverter(typeof(StringEnumConverter))]
public enum Colour
{
	Black,
	White,
	Red,
	Green,
	Blue
}
