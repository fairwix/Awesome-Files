using System.CommandLine;
using AwesomeFiles.Client.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace AwesomeFiles.Client.Commands;

public static class CreateArchiveCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("create-archive", "Create an archive from specified files");
        
        var filesArgument = new Argument<string[]>("file-names", "List of file names to archive")
        {
            Arity = ArgumentArity.OneOrMore
        };
        command.AddArgument(filesArgument);

        command.SetHandler(async (string[] fileNames) =>
        {
            var apiClient = services.GetRequiredService<IApiClient>();
            
            try
            {
                var response = await apiClient.CreateArchiveAsync(fileNames);
                Console.WriteLine($"Create archive task is started, id: {response.Id}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        }, filesArgument);

        return command;
    }
}