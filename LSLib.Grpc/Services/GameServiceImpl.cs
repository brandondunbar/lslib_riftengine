using System;
using System.Threading.Tasks;
using System.IO;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static LSLib.Grpc.GameService;
using LSLib.LS;
using LSLib.VirtualTextures;

namespace LSLib.Grpc.Services
{
    public class GameServiceImpl : GameServiceBase
    {
        // Declare an ILogger for logging
        private readonly ILogger<GameServiceImpl> _logger;

        // Dependency injection for ILogger
        // This allows the logger to be injected when this class is instantiated
        public GameServiceImpl(ILogger<GameServiceImpl> logger)
        {
            _logger = logger;
        }

        // Action delegate for shutdown requests
        public Action OnShutdownRequested { get; set; }

        // Override the Unpack method defined in GameServiceBase
        public override async Task<UnpackReply> Unpack(UnpackRequest request, ServerCallContext context)
        {
            // Default status to indicate success
            string status = "Unpacked";

            try
            {
                // Log the call to the Unpack method
                _logger.LogInformation("Unpack method called.");

                string rootGamePath = request.GamePath;
                string targetUnpackDirectory = request.TargetUnpackDirectory;

                // Navigate to the Data/ subdirectory
                string dataDirectory = Path.Combine(rootGamePath, "Data");

                // Get all .pak files in the Data/ subdirectory
                string[] pakFiles = Directory.GetFiles(dataDirectory, "*.pak");
                _logger.LogInformation($"Total .pak files to unpack: {pakFiles.Length}");


                // Initialize the Packager class from LSLib.LS
                var packager = new Packager();

                foreach (string pakFile in pakFiles)
                {
                    string pakFileName = Path.GetFileNameWithoutExtension(pakFile);
                    string targetDirectoryForThisPak = Path.Combine(targetUnpackDirectory, pakFileName);

                    // Create the target directory if it doesn't exist
                    Directory.CreateDirectory(targetDirectoryForThisPak);

                    _logger.LogInformation("Unpacking:" + pakFile + " to " + targetDirectoryForThisPak);

                    // Run the UncompressPackage method asynchronously
                    try
                    {
                        await Task.Run(() => packager.UncompressPackage(pakFile, targetDirectoryForThisPak));
                        _logger.LogInformation($"{targetDirectoryForThisPak}/{pakFile} unpacked");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to unpack {pakFile} to {targetDirectoryForThisPak}. Error: {ex.Message}");
                    }
                }
                _logger.LogInformation("Finished unpacking.");
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "Error occurred while unpacking.");

                // Update status to indicate failure
                status = "Failed";
            }

            // Return the status in a new UnpackReply object
            return new UnpackReply { Status = status };
        }

        // Override the Pack method defined in GameServiceBase
        public override Task<PackReply> Pack(PackRequest request, ServerCallContext context)
        {
            // Default status to indicate success
            string status = "Packed";

            try
            {
                // Log the call to the Pack method
                _logger.LogInformation("Pack method called.");

                // Implement your packing logic here
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "Error occurred while packing.");

                // Update status to indicate failure
                status = "Failed";
            }

            // Return the status in a new PackReply object
            return Task.FromResult(new PackReply { Status = status });
        }
    }
}
