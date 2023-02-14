FROM mcr.microsoft.com/dotnet/sdk:7.0.102-alpine3.17-amd64 AS build-env
COPY Messages/MattMS.TryRabbitMQ.Messages.fsproj /app/Messages/
COPY Worker/MattMS.TryRabbitMQ.Worker.fsproj /app/Worker/
WORKDIR /app/Worker
RUN dotnet restore
COPY Messages/*.fs /app/Messages/
COPY Worker/*.fs /app/Worker/
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:7.0.2-alpine3.17-amd64
WORKDIR /app
COPY --from=build-env /app/Worker/out .
ENTRYPOINT ["dotnet", "MattMS.TryRabbitMQ.Worker.dll"]
