using System;
using System.IO;
using System.ServiceProcess;
using LSLib.Grpc.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace LSLib.Grpc
{
    public static class Program
    {
        public static void Main()
        {
            // Get the directory of the currently executing assembly
            string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string assemblyDirectory = Path.GetDirectoryName(assemblyLocation);

            // Ensure the logs directory exists
            Directory.CreateDirectory(Path.Combine(assemblyDirectory, "logs"));

            // Construct the log file path
            string logFilePath = Path.Combine(assemblyDirectory, "logs", "lslibgrpc.txt");

            // Initialize Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            // Set up dependency injection
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder => builder.AddSerilog())
                .AddSingleton<GameServiceImpl>()
                .AddSingleton<GameServiceWindowsService>()
                .BuildServiceProvider();

            try
            {
                // Resolve the GameServiceWindowsService instance from the service provider
                var gameServiceWindowsService = serviceProvider.GetService<GameServiceWindowsService>();

                // Run the service
                ServiceBase.Run(gameServiceWindowsService);

                Log.Information("Service started successfully.");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "An error occurred while starting the service.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
