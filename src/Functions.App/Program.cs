using Functions.Domain.Models;
using Functions.Domain.Utilities;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Functions.App
{
	public static class Program
	{
		public static void Main()
		{
			// Add Enum serializer to Json converters:
			var jsonSerializerSettings = new JsonSerializerSettings();
			jsonSerializerSettings.Converters.Add(new EnumFlexConverter<Colour>());

			var host = new HostBuilder()
				.ConfigureFunctionsWorkerDefaults(worker => worker.UseNewtonsoftJson(jsonSerializerSettings))
				.ConfigureOpenApi()
				.Build();

			host.Run();
		}
	}
}