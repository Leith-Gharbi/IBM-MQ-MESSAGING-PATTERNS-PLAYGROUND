# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY src/MqPlayground.Api/MqPlayground.Api.csproj ./MqPlayground.Api/
RUN dotnet restore MqPlayground.Api/MqPlayground.Api.csproj

# Copy source and build
COPY src/MqPlayground.Api/. ./MqPlayground.Api/
WORKDIR /src/MqPlayground.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Expose port
EXPOSE 5000

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "MqPlayground.Api.dll"]
