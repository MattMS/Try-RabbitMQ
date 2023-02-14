module MattMS.TryRabbitMQ.API.Server

open System.Text.Json
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Giraffe
open RabbitMQ.Client

open MattMS.TryRabbitMQ.Messages

type LogTarget = class end

[<CLIMutable>]
type ChannelOptions =
    {
        Exchange: string
        Queue: string
        RoutingKey: string
    }

let getChannelSettings (sectionName: string) (configuration: IConfiguration) =
    let section = configuration.GetSection sectionName
    {
        Exchange = section.GetValue "Exchange"
        Queue = section.GetValue "Queue"
        RoutingKey = section.GetValue "RoutingKey"
    }

let logFromContext (message: string) (ctx: HttpContext) =
    // let logger = ctx.GetService<ILogger<LogTarget>>()
    let logger = ctx.GetLogger<LogTarget>()
    logger.LogInformation(message)
    ctx

let send (channel: IModel) (message: string) =
    let messageBytes = System.Text.Encoding.UTF8.GetBytes(message)

    // printfn "%A" <| getChannelSettings "Channel" ctx.Configuration

    // open Microsoft.Extensions.Options
    // let channelOptions = ctx.GetService<IOptions<ChannelOptions>>().Value
    let channelOptions =
        {
            Exchange = ""
            Queue = "hello"
            RoutingKey = "hello"
        }

    let timestamp = AmqpTimestamp(System.DateTimeOffset.UtcNow.ToUnixTimeSeconds())
    let basicProperties = channel.CreateBasicProperties(ContentType="application/json", Timestamp=timestamp)
    // AppId
    // CorrelationId
    // Expiration
    // Type
    // UserId

    channel.QueueDeclare(arguments=null, autoDelete=false, durable=false, exclusive=false, queue=channelOptions.Queue) |> ignore
    // channel.BasicQos(``global``=false, prefetchCount=1us, prefetchSize=0u)
    channel.BasicPublish(basicProperties=basicProperties, body=messageBytes, exchange=channelOptions.Exchange, routingKey=channelOptions.RoutingKey)

let getHome: HttpHandler =
    handleContext(fun ctx ->
        let logger = ctx.GetLogger<LogTarget>()
        logger.LogInformation("Home hit")

        let connection = ctx.GetService<IConnection>()
        use channel = connection.CreateModel()

        let event: PageHitEvent = {Id = ctx.Connection.Id}
        let message = JsonSerializer.Serialize(event)
        send channel message

        let name = ctx.GetQueryStringValue "name" |> Result.defaultValue "buddy"
        let greeting = sprintf "Hi %s" name

        ctx.WriteTextAsync greeting
    )

// Main

let webApp =
    route "/" >=> getHome

[<CLIMutable>]
type RabbitMQOptions =
    {
        Host: string
        Pass: string
        User: string
    }

let configureApp (app: WebApplication) =
    app.MapHealthChecks("/healthz") |> ignore

    app.UseGiraffe webApp

    app

let configureBuilder (builder: WebApplicationBuilder) =
    // Alternate access: `builder.Configuration.GetSection("RabbitMQ").GetValue("Host")`
    let rabbitMQSection = builder.Configuration.GetSection("RabbitMQ").Get<RabbitMQOptions>()
    let factory = ConnectionFactory(
        HostName=rabbitMQSection.Host,
        Password=rabbitMQSection.Pass,
        UserName=rabbitMQSection.User
    )
    builder.Services.AddSingleton<IConnection>(fun _ -> factory.CreateConnection()) |> ignore

    // builder.Services.Configure<ChannelOptions>(builder.Configuration.GetSection("Channel")) |> ignore

    builder.Services.AddGiraffe() |> ignore

    builder.Services.AddHealthChecks() |> ignore

    builder

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args) |> configureBuilder
    let app = builder.Build() |> configureApp
    app.Run()
    0
