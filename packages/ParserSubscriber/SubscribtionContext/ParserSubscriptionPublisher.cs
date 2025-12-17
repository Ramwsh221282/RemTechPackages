using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ParserSubscriber.SubscribtionContext.Options;
using RabbitMQ.Client;
using Serilog;

namespace ParserSubscriber.SubscribtionContext;

public sealed class ParserSubscriptionPublisher(
    RabbitMqConnectionProvider rabbitMq,
    IOptions<RabbitMqRequestReplyResponseListeningQueueOptions> options,
    ILogger? logger)
{
    private RabbitMqConnectionProvider RabbitMq { get; } = rabbitMq;
    private RabbitMqRequestReplyResponseListeningQueueOptions Options { get; } = options.Value;
    private ILogger? Logger { get; } = logger;

    internal async Task Publish(ParserSubscribtion subscription)
    {
        Logger?.Information("Publishing subscribe parser message");

        Options.RequestQueueInfo(out var queue, out var exchange, out var routingKey);

        var connection = await RabbitMq();

        CreateChannelOptions channelOptions = new(
            true,
            true
        );

        await using var channel = await connection.CreateChannelAsync(channelOptions);

        await channel.ExchangeDeclareAsync(
            exchange,
            "topic",
            durable: true,
            autoDelete: false
        );

        await channel.QueueDeclareAsync(
            queue,
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        await channel.QueueBindAsync(
            queue,
            exchange,
            routingKey
        );

        object payload = new { parser_id = subscription.Id, parser_type = subscription.Type, parser_domain = subscription.Domain };
        string jsonPayload = JsonSerializer.Serialize(payload);
        ReadOnlyMemory<byte> bytesPayload = Encoding.UTF8.GetBytes(jsonPayload);

        BasicProperties publishOptions = new() { Persistent = true };

        await channel.BasicPublishAsync(
            exchange,
            routingKey,
            true,
            publishOptions,
            bytesPayload
        );
    }
}