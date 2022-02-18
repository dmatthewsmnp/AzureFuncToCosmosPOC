using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Functions.App
{
	public static class Program
	{
		public static void Main()
		{
			var host = new HostBuilder()
				.ConfigureFunctionsWorkerDefaults()
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