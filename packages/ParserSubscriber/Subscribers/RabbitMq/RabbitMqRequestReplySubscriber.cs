using System.Text;
using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ParserSubscriber.Subscribers.RabbitMq;

public delegate Task<IConnection> RabbitMqConnectionSource(CancellationToken ct = default);
public delegate Task RabbitMqRequestReplySubscriberMessageHandler(BasicDeliverEventArgs ea, IChannel channel);

public static class ParserSubscriberInjection
{
    extension(IServiceCollection services)
    {
        public void RegisterParserSubscriber<T>(
            Action<IServiceCollection> rabbitMqProviderConfiguration,
            Action<IServiceCollection> messageHandlerConfiguration,
            Action<IServiceCollection> optionsConfiguration
        ) where T : class, IParserSubscriber
        {
            rabbitMqProviderConfiguration(services);
            messageHandlerConfiguration(services);
            optionsConfiguration(services);
            services.AddTransient<IParserSubscriber, T>();
        }
    }
}

public sealed class RabbitMqRequestReplySubscriber : IParserSubscriber
{
    private Serilog.ILogger? Logger { get; }
    private TaskCompletionSource Tcs { get; } = new();
    private IChannel ResponseQueueChannel { get; set; } = null!;
    private AsyncEventingBasicConsumer Consumer { get; set; } = null!;
    private IOptions<RabbitMqRequestReplyResponseListeningQueueOptions> RerpOpts { get; }
    private RabbitMqRequestReplySubscriberMessageHandler HandlerMethod { get; }
    private AsyncEventHandler<BasicDeliverEventArgs> EventHandler { get; }
    private RabbitMqConnectionSource RabbitMq { get; }

    public RabbitMqRequestReplySubscriber(
        RabbitMqConnectionSource rabbitMq,
        RabbitMqRequestReplySubscriberMessageHandler handler,
        IOptions<RabbitMqRequestReplyResponseListeningQueueOptions> options,
        Serilog.ILogger? logger
        )
    {
        Logger = logger?.ForContext<RabbitMqRequestReplySubscriber>();
        RerpOpts = options;
        HandlerMethod = handler;
        EventHandler = Handler();
        RabbitMq = rabbitMq;
    }

    public async Task<ParserSubscribtion> Subscribe(CancellationToken ct = default)
    {
        await CreateTemporaryResponseListeningQueue(ct);
        ParserSubscribtion subscribtion = new(Guid.NewGuid(), DateTime.Now);
        await PublishSubscriptionMessage(subscribtion, ct);
        await Tcs.Task;
        return subscribtion;
    }

    private async Task PublishSubscriptionMessage(ParserSubscribtion subscribtion, CancellationToken ct)
    {
        Logger?.Information("Publishing subscribtion message");
        RerpOpts.Value.ParserInfo(out string domain, out string type);

        Guid id = subscribtion.Id;
        string parser_domain = domain;
        string parser_type = type;

        Logger?.Information(
            """
            Subscribtion message info:
            Id: {id}
            Parser domain: {parser_domain}
            Parser type: {parser_type}
            """, id, parser_domain, parser_type);

        object payload = new
        {
            id,
            parser_domain,
            parser_type
        };

        string json = JsonSerializer.Serialize(payload);
        ReadOnlyMemory<byte> message = Encoding.UTF8.GetBytes(json);

        RerpOpts.Value.RequestQueueInfo(out string queue, out string exchange, out string routingKey);
        object[] logProperties = [queue, exchange, routingKey];

        Logger?.Information(
            """
            Request queue settings info:
            Queue: {queue}
            Exchange: {exchange}
            Routing key: {routingKey}
            """, logProperties);

        await using IChannel channel = await (await RabbitMq(ct)).CreateChannelAsync(cancellationToken: ct);
        await channel.ExchangeDeclareAsync(exchange: exchange, type: ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: ct);
        await channel.QueueDeclareAsync(queue: queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
        await channel.QueueBindAsync(queue: queue, exchange: exchange, routingKey: routingKey, cancellationToken: ct);

        BasicProperties publishProperties = new() { Persistent = true };

        await channel.BasicPublishAsync(exchange: exchange, routingKey: routingKey, mandatory: true, basicProperties: publishProperties, body: message, cancellationToken: ct);

        Logger?.Information("Subscribtion message published");
    }

    private async Task CreateTemporaryResponseListeningQueue(CancellationToken ct)
    {
        Logger?.Information("Creating temporary response listener queue");

        RerpOpts.Value.ResponseQueueInfo(out string queue, out string exchange, out string routingKey);
        object[] logProperties = [queue, exchange, routingKey];

        Logger?.Information(
            """
            Response listener settings info:
            Queue: {queue}
            Exchange: {exchange}
            Routing key: {routingKey}
            """, logProperties);

        CreateChannelOptions channelOpts = new(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true);
        ResponseQueueChannel = await (await RabbitMq(ct)).CreateChannelAsync(channelOpts, ct);
        await ResponseQueueChannel.ExchangeDeclareAsync(exchange: exchange, type: ExchangeType.Topic, cancellationToken: ct);
        await ResponseQueueChannel.QueueDeclareAsync(queue: queue, cancellationToken: ct);
        await ResponseQueueChannel.QueueBindAsync(queue: queue, exchange: exchange, routingKey: routingKey, cancellationToken: ct);

        Logger?.Information("Temporary response listener queue created");
        Logger?.Information("Attaching AsyncEventingBasicConsumer for temporary response listener queue");

        Consumer = new AsyncEventingBasicConsumer(ResponseQueueChannel);
        Consumer.ReceivedAsync += EventHandler;
        await ResponseQueueChannel.BasicConsumeAsync(queue: queue, autoAck: true, consumer: Consumer, cancellationToken: ct);

        Logger?.Information("Reply queue is ready.");
    }

    private AsyncEventHandler<BasicDeliverEventArgs> Handler() => async (_, ea) =>
    {
        Logger?.Information("Received response message. Invoking handler method.");
        try
        {
            await HandlerMethod(ea, ResponseQueueChannel);
            Logger?.Information("Handler method finished.");
            Tcs.SetResult();
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "Handler method resulted in error.");
            Tcs.SetException(ex);
        }
        finally
        {
            await DestroyResponseQueueConsumer();
        }
    };

    private async Task DestroyResponseQueueConsumer()
    {
        RerpOpts.Value.ResponseQueueInfo(out string queue, out string exchange, out string routingKey);
        await ResponseQueueChannel.QueueUnbindAsync(queue: queue, exchange: exchange, routingKey: routingKey);
        await ResponseQueueChannel.QueuePurgeAsync(queue: queue);
        await ResponseQueueChannel.ExchangeDeleteAsync(exchange: exchange);
        Consumer.ReceivedAsync -= EventHandler;
        await ResponseQueueChannel.DisposeAsync();
    }
}
