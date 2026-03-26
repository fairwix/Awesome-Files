using System.CommandLine;
using AwesomeFiles.Client.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace AwesomeFiles.Client.Commands;

public static class StatusCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("status", "Check archive creation status");
        
        var idArgument = new Argument<Guid>("id", "Task identifier");
        command.AddArgument(idArgument);

        command.SetHandler(async (Guid taskId) =>
        {
            var apiClient = services.GetRequiredService<IApiClient>();
            
            try
            {
                var status = await apiClient.GetStatusAsync(taskId);
                
                var message = status.Status switch
                {
                    "Pending" => "Process pending, please wait...",
                    "InProgress" => "Process in progress, please wait...",
                    "Completed" => "Archive has been created.",
                    "Failed" => $"Archive creation failed: {status.Error}",
                    _ => $"Status: {status.Status}"
                };
                
                Console.WriteLine(message);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        }, idArgument);

        return command;
    }
}