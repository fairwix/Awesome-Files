using AwesomeFiles.Application.Services;
using AwesomeFiles.Application.UseCases;

namespace AwesomeFiles.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IGetFilesUseCase, GetFilesUseCase>();
        services.AddScoped<ICreateArchiveUseCase, CreateArchiveUseCase>();
        services.AddScoped<IGetArchiveStatusUseCase, GetArchiveStatusUseCase>();
        services.AddScoped<IDownloadArchiveUseCase, DownloadArchiveUseCase>();
        
        return services;
    }
}