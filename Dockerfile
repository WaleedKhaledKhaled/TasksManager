# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /src

COPY TasksManager.sln ./
COPY global.json ./
COPY src/TasksManager.Api/TasksManager.Api.csproj src/TasksManager.Api/
COPY tests/TasksManager.Api.Tests/TasksManager.Api.Tests.csproj tests/TasksManager.Api.Tests/
RUN dotnet restore TasksManager.sln

COPY . .
RUN dotnet publish src/TasksManager.Api/TasksManager.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish ./
COPY docker/entrypoint.sh ./
RUN chmod +x /app/entrypoint.sh

EXPOSE 8080

ENTRYPOINT ["/app/entrypoint.sh"]
