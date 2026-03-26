using AwesomeFiles.Application.Interfaces;
using AwesomeFiles.Application.Interfaces.Services;
using AwesomeFiles.Infrastructure.BackgroundServices;
using AwesomeFiles.Infrastructure.Options;
using AwesomeFiles.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options; 


namespace AwesomeFiles.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<FileStorageOptions>(
            configuration.GetSection("FileStorage"));
        
        services.Configure<ArchiveStorageOptions>(
            configuration.GetSection("ArchiveStorage"));
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IArchiveService, ArchiveService>();
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<ArchiveWorker>();
        
        return services;
    }
}