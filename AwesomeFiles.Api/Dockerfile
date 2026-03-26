# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files
COPY AwesomeFiles.Domain/*.csproj AwesomeFiles.Domain/
COPY AwesomeFiles.Application/*.csproj AwesomeFiles.Application/
COPY AwesomeFiles.Infrastructure/*.csproj AwesomeFiles.Infrastructure/
COPY AwesomeFiles.Api/*.csproj AwesomeFiles.Api/
COPY AwesomeFiles.Tests/*.csproj AwesomeFiles.Tests/

# Restore dependencies
RUN dotnet restore AwesomeFiles.Api/AwesomeFiles.Api.csproj

# Copy all source code
COPY . .

# Build and publish
WORKDIR /src/AwesomeFiles.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Expose ports
EXPOSE 80
EXPOSE 443

# Copy published application
COPY --from=build /app/publish .

# Create directories for files and archives
RUN mkdir -p /app/Files /app/Archives

RUN adduser --disabled-password --gecos "" appuser \
    && chown -R appuser:appuser /app

USER appuser

# Set entry point
ENTRYPOINT ["dotnet", "AwesomeFiles.Api.dll"]