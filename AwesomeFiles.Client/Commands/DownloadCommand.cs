using System.CommandLine;
using AwesomeFiles.Client.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace AwesomeFiles.Client.Commands;

public static class DownloadCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("download", "Download a completed archive");
        
        var idArgument = new Argument<Guid>("id", "Task identifier");
        command.AddArgument(idArgument);
        
        var pathArgument = new Argument<string>("destination-folder", () => ".", "Folder to save the archive");
        command.AddArgument(pathArgument);

        command.SetHandler(async (Guid taskId, string destinationFolder) =>
        {
            var apiClient = services.GetRequiredService<IApiClient>();
            
            try
            {
                await apiClient.DownloadArchiveAsync(taskId, destinationFolder);
                Console.WriteLine($"Archive downloaded to {destinationFolder}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        }, idArgument, pathArgument);

        return command;
    }
}