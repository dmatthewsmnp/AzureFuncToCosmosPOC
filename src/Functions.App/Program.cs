using Functions.Domain.Models;
using Functions.Domain.Utilities;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
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
				.ConfigureServices(services =>
				{
					services.AddSingleton(new CosmosClient(System.Environment.GetEnvironmentVariable("CosmosDBConnection")));
				})
				.ConfigureOpenApi()
				.Build();

			host.Run();
		}
	}
}