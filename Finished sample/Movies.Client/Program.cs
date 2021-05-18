using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Movies.Client.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Movies.Client
{
    class Program
    { 
        static async Task Main(string[] args)
        {

            using IHost host = CreateHostBuilder(args).Build();         
            var serviceProvider = host.Services; 
            
            // For demo purposes: overall catch-all to log any exception that might 
            // happen to the console & wait for key input afterwards so we can easily 
            // inspect the issue.  
            try
            {
                var logger = host.Services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Host created.");

                // Run our IntegrationService containing all samples and
                // await this call to ensure the application doesn't 
                // prematurely exit.
                await serviceProvider.GetService<IIntegrationService>().Run();
            }
            catch (Exception generalException)
            {
                // log the exception
                var logger = serviceProvider.GetService<ILogger<Program>>();
                logger.LogError(generalException, 
                    "An exception happened while running the integration service.");
            }
            
            Console.ReadKey();

            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args).ConfigureServices(
                (serviceCollection) => ConfigureServices(serviceCollection)); 
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // add loggers           
            serviceCollection.AddLogging(configure => configure.AddDebug().AddConsole());

            serviceCollection.AddHttpClient("MoviesClient", client =>
            {
                client.BaseAddress = new Uri("http://localhost:57863");
                client.Timeout = new TimeSpan(0, 0, 30);
                client.DefaultRequestHeaders.Clear();
            }).AddHttpMessageHandler(handler =>
                new TimeOutDelegatingHandler(TimeSpan.FromSeconds(20)))
                .AddHttpMessageHandler(handler => 
                new RetryPolicyDelegatingHandler(2))
                .ConfigurePrimaryHttpMessageHandler(handler =>
                new HttpClientHandler()
                {
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip
                });

            //serviceCollection.AddHttpClient<MoviesClient>(client =>
            //{
            //    client.BaseAddress = new Uri("http://localhost:57863");
            //    client.Timeout = new TimeSpan(0, 0, 30);
            //    client.DefaultRequestHeaders.Clear();
            //}
            //).ConfigurePrimaryHttpMessageHandler(handler =>
            //new HttpClientHandler()
            //{
            //    AutomaticDecompression = System.Net.DecompressionMethods.GZip
            //});

            serviceCollection.AddHttpClient<MoviesClient>()
                .ConfigurePrimaryHttpMessageHandler(handler =>
           new HttpClientHandler()
           {
               AutomaticDecompression = System.Net.DecompressionMethods.GZip
           });


            // register the integration service on our container with a 
            // scoped lifetime

            // For the CRUD demos
            // serviceCollection.AddScoped<IIntegrationService, CRUDService>();

            // For the partial update demos
            // serviceCollection.AddScoped<IIntegrationService, PartialUpdateService>();

            // For the stream demos
            // serviceCollection.AddScoped<IIntegrationService, StreamService>();

            // For the cancellation demos
            // serviceCollection.AddScoped<IIntegrationService, CancellationService>();

            // For the HttpClientFactory demos
            // serviceCollection.AddScoped<IIntegrationService, HttpClientFactoryInstanceManagementService>();

            // For the dealing with errors and faults demos
            // serviceCollection.AddScoped<IIntegrationService, DealingWithErrorsAndFaultsService>();

            // For the custom http handlers demos
            serviceCollection.AddScoped<IIntegrationService, HttpHandlersService>();     
        }
    }
}
