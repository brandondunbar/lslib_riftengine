// Import necessary namespaces
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Grpc.Core;
using Grpc.Core.Interceptors;
using LSLib.Grpc.Services;
using Serilog;
using System.IO;
using System;

namespace LSLib.Grpc
{
    // Define the Startup class
    public class Startup
    {
        // Declare a property for IConfiguration
        public IConfiguration Configuration { get; }

        // Constructor that initializes the Configuration property
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // Method to configure services
        public void ConfigureServices(IServiceCollection services)
        {
            // Initialize Serilog for logging
            // Logs will be written to "logs\\lslibwrapperlogs.txt" and a new file will be created every day
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("logs\\lslibwrapperlogs.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            // Add Serilog as the logging provider
            services.AddLogging(builder => builder.AddSerilog());

            // Register GameServiceImpl as a singleton service
            // This ensures that only one instance of GameServiceImpl will be created
            services.AddSingleton<GameServiceImpl>();

            // Register GameServiceWindowsService as a singleton service
            // This ensures that only one instance of GameServiceWindowsService will be created
            services.AddSingleton<GameServiceWindowsService>();
        }


        // Method to configure and start the gRPC server
        public void ConfigureServer(IServiceProvider serviceProvider)
        {
            // Retrieve the logger service
            var logger = serviceProvider.GetService<ILogger<Startup>>();

            // Retrieve server settings from configuration or use default values
            var serverAddress = Configuration["Server:Address"] ?? "localhost";
            var serverPort = int.Parse(Configuration["Server:Port"] ?? "50051");

            // Retrieve the GameServiceImpl instance from the service provider
            var gameService = serviceProvider.GetService<GameServiceImpl>();

            // Retrieve the GameServiceWindowsService instance from the service provider
            var gameServiceWindowsService = serviceProvider.GetService<GameServiceWindowsService>();

            // Initialize and configure the gRPC server
            Server server = new Server
            {
                Services = { GameService.BindService(gameService).Intercept(new ServerLoggerInterceptor(logger)) },
                Ports = { new ServerPort(serverAddress, serverPort, ServerCredentials.Insecure) }
            };

            // Start the gRPC server
            server.Start();

            // Log the server start information
            logger.LogInformation($"Server started at {serverAddress}:{serverPort}");
        }

    }

    // Define the ServerLoggerInterceptor class
    public class ServerLoggerInterceptor : Interceptor
    {
        // Declare a logger property
        private readonly ILogger<Startup> _logger;

        // Constructor that initializes the logger property
        public ServerLoggerInterceptor(ILogger<Startup> logger)
        {
            _logger = logger;
        }
    }
}
