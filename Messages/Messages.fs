namespace MattMS.TryRabbitMQ.Messages

type PageHitEvent =
    {
        Id: string
    }

type DoWorkRequest =
    {
        Request: string
    }

type DoWorkResponse =
    {
        Reply: string
    }
