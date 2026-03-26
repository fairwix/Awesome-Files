using System.CommandLine;
using AwesomeFiles.Client.Application;
using Microsoft.Extensions.DependencyInjection;

namespace AwesomeFiles.Client.Commands;

public static class AutoArchiveCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("auto", "Create archive, wait for completion, and download in one command");
        
        var filesArgument = new Argument<string[]>("file-names", "List of file names to archive")
        {
            Arity = ArgumentArity.OneOrMore
        };
        command.AddArgument(filesArgument);
        
        var pathArgument = new Argument<string>("destination-folder", () => ".", "Folder to save the archive");
        command.AddArgument(pathArgument);

        command.SetHandler(async (string[] fileNames, string destinationFolder) =>
        {
            var archiveService = services.GetRequiredService<IArchiveClientService>();
            
            try
            {
                await archiveService.CreateAndWaitAndDownloadAsync(fileNames, destinationFolder);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        }, filesArgument, pathArgument);

        return command;
    }
}