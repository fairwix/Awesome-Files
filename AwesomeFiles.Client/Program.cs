using System.CommandLine;
using AwesomeFiles.Client.Application;
using AwesomeFiles.Client.Commands;
using AwesomeFiles.Client.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Configure typed HTTP client
services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    var baseUrl = Environment.GetEnvironmentVariable("AWESOME_FILES_API_URL") ?? "http://localhost:5083";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register application services
services.AddScoped<IArchiveClientService, ArchiveClientService>();

var serviceProvider = services.BuildServiceProvider();

// Create root command
var rootCommand = new RootCommand("Awesome Files CLI client - interactive tool for file archiving");

// Add all commands
rootCommand.AddCommand(ListCommand.Create(serviceProvider));
rootCommand.AddCommand(CreateArchiveCommand.Create(serviceProvider));
rootCommand.AddCommand(StatusCommand.Create(serviceProvider));
rootCommand.AddCommand(DownloadCommand.Create(serviceProvider));
rootCommand.AddCommand(AutoArchiveCommand.Create(serviceProvider));

// Parse and invoke
return await rootCommand.InvokeAsync(args);