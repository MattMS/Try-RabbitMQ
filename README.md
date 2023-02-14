# RabbitMQ experiment

This repo contains my experiments with trying to get a .NET Solution with RabbitMQ working on Azure.

## Code overview

The Solution is very simple: a [Giraffe web server][Giraffe], a .NET worker service, and a shared messages library.

The Dockerfiles are only used for building the projects for cloud deployment, while `dotnet watch` is used for local development.

The base images are all official:

- Everything builds with the same SDK: [`mcr.microsoft.com/dotnet/sdk:7.0-alpine`](https://hub.docker.com/_/microsoft-dotnet-sdk/)
- Worker uses the normal runtime: [`mcr.microsoft.com/dotnet/runtime:7.0-alpine`](https://hub.docker.com/_/microsoft-dotnet-runtime/)
- Site uses the ASP.NET runtime: [`mcr.microsoft.com/dotnet/aspnet:7.0-alpine`](https://hub.docker.com/_/microsoft-dotnet-aspnet/)

[Giraffe]: https://giraffe.wiki/

## Prerequisites

- Since this uses Docker Compose, you likely need [Docker Desktop](https://www.docker.com/products/docker-desktop/).
  **IMPORTANT:** Please review the licensing costs/terms if this is for business use.
- For development, you need the [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download)
- You are strongly encouraged to use the latest stable release of [Posh](https://github.com/PowerShell/PowerShell/releases)

## Getting started

To start local development, you can use `up.cmd`, which runs the following:

```
docker compose -f compose.yaml -f compose.local.yaml up --build -d
```

You will then need to open terminals in both the `Server` and `Worker` folders and run `dotnet watch` on each.

The web server can be accessed on [port 5000](http://localhost:5000) and the RabbitMQ console is on [port 15672](http://localhost:15672).

When you are finished, kill the `dotnet watch` jobs and then run `down.cmd`, which does the following:

```
docker compose -f compose.yaml -f compose.local.yaml down
```

### Setting up for Azure

**REMEMBER:** Take note of whatever you choose for the uppercase names in this section, as they are needed later.

From your Posh terminal, install the Az module:

```
inmo az
```

In future sessions, you can use `ipmo az` to `Import-Module`, instead of `inmo az` (`Install-Module`).

To work with Azure, you obviously need an account, so I assume that has been done and you are logged into it in your default browser.
Now you need to connect your terminal session to it:

```
Connect-AzAccount
```

Create a Resource Group for all this junk:

```
New-AzResourceGroup -Location australiasoutheast -Name MY-RESOURCE-GROUP
```

Pick a fancy, globally-unique name to create a Container Registry for your Docker Images:

```
New-AzContainerRegistry -Name MY-REGISTRY -ResourceGroupName MY-RESOURCE-GROUP -Sku Basic
```

You need a Docker Context that is connected to your Azure account:

```
docker context create aci MY-AZURE-CONTEXT
```

### Azure

To be able to push images, you must be in your default context (rather than the Azure one you just created):

```
docker context use default
```

The Azure registry seems to complain about Docker Hub images, so they need to be tagged and pushed:

```
docker tag rabbitmq:3.11.9-management-alpine MY-REGISTRY.azurecr.io/rabbitmq:3.11.9-management-alpine

docker push MY-REGISTRY.azurecr.io/rabbitmq:3.11.9-management-alpine
```

Then push the service images:

```
docker compose -f compose.yaml -f compose.azure.yaml push
```

Now switch to your Azure context:

```
docker context use MY-AZURE-CONTEXT
```

You can finally start the containers to see all your hard work:

```
docker compose -f compose.yaml -f compose.azure.yaml up -d
```

Use `docker ps -a` to find the IP address for the server container, and then you can navigate to this in your browser.

When you're finished:

```
docker compose -f compose.yaml -f compose.azure.yaml down
```

## Documentation

- [Tutorial: Deploy a multi-container group using Docker Compose](https://learn.microsoft.com/en-au/azure/container-instances/tutorial-docker-compose)

### Giraffe

- [Giraffe docs](https://giraffe.wiki/docs)

### RabbitMQ on .NET

- [NuGet](https://www.nuget.org/packages/RabbitMQ.Client)
- [API documentation](https://rabbitmq.github.io/rabbitmq-dotnet-client/index.html)
- [API guide](https://www.rabbitmq.com/dotnet-api-guide.html)
- [`ConnectionFactory`](https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.ConnectionFactory.html)
