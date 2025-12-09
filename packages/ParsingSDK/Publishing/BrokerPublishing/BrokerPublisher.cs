using System.Text;
using RabbitMQ.Client;

namespace ParsingSDK.Publishing.BrokerPublishing;

public sealed class BrokerPublisher(Func<CancellationToken, Task<IConnection>> connectionFactory)
    : IPublisher<BrokerPublishingOptions>
{
    public async Task Publish(
        string content, 
        BrokerPublishingOptions options, 
        CancellationToken ct = default)
    {
        IConnection connection = await connectionFactory(ct);

        CreateChannelOptions publishOptions = new(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);
        
        await using IChannel channel = await connection.CreateChannelAsync(
            options: publishOptions,
            cancellationToken: ct
        );

        await channel.QueueDeclareAsync(
            queue: options.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: ct);

        await channel.ExchangeDeclareAsync(
            exchange: options.Exchange,
            type: options.Type,
            durable: true,
            autoDelete: false,
            cancellationToken: ct);

        await channel.QueueBindAsync(
            queue: options.Queue,
            exchange: options.Exchange,
            routingKey: options.RoutingKey,
            cancellationToken: ct);

        BasicProperties publishProperties = new() { Persistent = true };

        ReadOnlyMemory<byte> body = Encoding.UTF8.GetBytes(content);

        await channel.BasicPublishAsync(
            exchange: options.Exchange,
            routingKey: options.RoutingKey,
            mandatory: true,
            basicProperties: publishProperties,
            body: body,
            cancellationToken: ct);
    }
}