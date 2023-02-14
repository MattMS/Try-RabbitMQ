module MattMS.TryRabbitMQ.API.Worker

open System.Threading
open System.Threading.Tasks
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open RabbitMQ.Client
open RabbitMQ.Client.Events

open MattMS.TryRabbitMQ.Messages

[<CLIMutable>]
type ChannelOptions =
    {
        Exchange: string
        Queue: string
        RoutingKey: string
    }

// let pageHitQueueName = "PageHit"

type Worker(connection: IConnection, logger: ILogger<Worker>) =
    // https://learn.microsoft.com/en-au/dotnet/fsharp/language-reference/inheritance
    inherit BackgroundService()

    // https://learn.microsoft.com/en-au/dotnet/api/microsoft.extensions.hosting.backgroundservice.executeasync?view=dotnet-plat-ext-7.0
    override _.ExecuteAsync(ct: CancellationToken) =
        task {
            printfn "Here"
            use channel = connection.CreateModel()

            // let channelOptions = ctx.GetService<IOptions<ChannelOptions>>().Value
            let channelOptions =
                {
                    Exchange = ""
                    Queue = "hello"
                    RoutingKey = "hello"
                }
            channel.QueueDeclare(arguments=null, autoDelete=false, durable=false, exclusive=false, queue=channelOptions.Queue) |> ignore

            let consumer = EventingBasicConsumer(channel)
            consumer.Received.Add(fun ea ->
                let messageBytes = ea.Body.ToArray()
                let message = System.Text.Encoding.UTF8.GetString(messageBytes)
                logger.LogInformation(message)
            )

            channel.BasicConsume(autoAck=true, consumer=consumer, queue=channelOptions.Queue) |> ignore
            while not ct.IsCancellationRequested do
                do! Task.Delay(1000)
        }

[<CLIMutable>]
type RabbitMQOptions =
    {
        Host: string
        Pass: string
        User: string
    }

[<EntryPoint>]
let main args =
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(fun context services ->
            let rabbitMQSection = context.Configuration.GetSection("RabbitMQ").Get<RabbitMQOptions>()
            let factory = ConnectionFactory(
                HostName=rabbitMQSection.Host,
                Password=rabbitMQSection.Pass,
                UserName=rabbitMQSection.User
            )
            services.AddSingleton<IConnection>(fun _ -> factory.CreateConnection()) |> ignore

            services.AddHostedService<Worker>() |> ignore
        )
        .Build()
        .Run()
    0
