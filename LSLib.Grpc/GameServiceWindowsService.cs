using System;
using System.ServiceProcess;
using Grpc.Core;
using LSLib.Grpc.Services;
using Microsoft.Extensions.Logging;

namespace LSLib.Grpc
{
    public class GameServiceWindowsService : ServiceBase
    {
        private readonly GameServiceImpl _gameService;
        private readonly ILogger<GameServiceWindowsService> _logger;
        private Server _server;

        public GameServiceWindowsService(GameServiceImpl gameService, ILogger<GameServiceWindowsService> logger)
        {
            _gameService = gameService;
            _logger = logger;
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _server = new Server
                {
                    Services = { GameService.BindService(_gameService) },
                    Ports = { new ServerPort("localhost", 50051, ServerCredentials.Insecure) }
                };
                _server.Start();
                _logger.LogInformation("Service started successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while starting the service.");
            }
        }

        protected override void OnStop()
        {
            try
            {
                _server?.ShutdownAsync().GetAwaiter().GetResult();
                _logger.LogInformation("Service stopped successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while stopping the service.");
            }
        }
    }
}
