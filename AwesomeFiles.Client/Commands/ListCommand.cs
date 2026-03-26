using System.CommandLine;
using AwesomeFiles.Client.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace AwesomeFiles.Client.Commands;

public static class ListCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("list", "Get list of all available files");

        command.SetHandler(async () =>
        {
            var apiClient = services.GetRequiredService<IApiClient>();
            
            try
            {
                var files = await apiClient.GetFilesAsync();
                Console.WriteLine(string.Join(" ", files));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        });

        return command;
    }
}