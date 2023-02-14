FROM mcr.microsoft.com/dotnet/sdk:7.0.102-alpine3.17-amd64 AS build-env
COPY Messages/MattMS.TryRabbitMQ.Messages.fsproj /app/Messages/
COPY Server/MattMS.TryRabbitMQ.Server.fsproj /app/Server/
WORKDIR /app/Server
RUN dotnet restore
COPY Messages/*.fs /app/Messages/
COPY Server/*.fs /app/Server/
COPY Server/appsettings.json /app/Server/
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0.2-alpine3.17-amd64
WORKDIR /app
COPY --from=build-env /app/Server/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "MattMS.TryRabbitMQ.Server.dll"]
