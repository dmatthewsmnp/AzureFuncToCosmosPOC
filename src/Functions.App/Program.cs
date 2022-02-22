using System.Threading.Tasks;
using Functions.App.Utilities;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Functions.App
{
	public static class Program
	{
		public static async Task Main()
		{
			var host = new HostBuilder()
				.ConfigureFunctionsWorkerDefaults(worker => worker.UseNewtonsoftJson()) // Note: without NewtonsoftJson, overflow JSON elements are not properly deserialized
				.ConfigureServices(services =>
				{
					services.AddSingleton(new CosmosDbUtils(
						System.Environment.GetEnvironmentVariable("CosmosDBConnection"),
						System.Environment.GetEnvironmentVariable("DBName")));
				})
				.ConfigureOpenApi()
				.Build();

			await host.RunAsync();
		}
	}
}