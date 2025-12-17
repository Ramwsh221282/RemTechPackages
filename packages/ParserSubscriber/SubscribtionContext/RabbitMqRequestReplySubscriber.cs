using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ParserSubscriber.SubscribtionContext.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace ParserSubscriber.SubscribtionContext;

public sealed class RabbitMqRequestReplySubscriber : IParserSubscriber
{
    public RabbitMqRequestReplySubscriber(
        RabbitMqConnectionProvider rabbitMq,
        ParserSubscriptionPublisher publisher,
        IOptions<RabbitMqRequestReplyResponseListeningQueueOptions> options,
        SubscriptionStorage storage,
        ILogger? logger
    )
    {
        Logger = logger?.ForContext<RabbitMqRequestReplySubscriber>();
        RerpOpts = options.Value;
        EventHandler = Handler();
        Publisher = publisher;
        RabbitMq = rabbitMq;
        Storage = storage;
    }
    
    private ILogger? Logger { get; }
    private TaskCompletionSource Tcs { get; } = new();
    private IChannel ResponseQueueChannel { get; set; } = null!;
    private AsyncEventingBasicConsumer Consumer { get; set; } = null!;
    private RabbitMqRequestReplyResponseListeningQueueOptions RerpOpts { get; }
    private ParserSubscriptionPublisher Publisher { get; }
    private SubscriptionStorage Storage { get; }
    private AsyncEventHandler<BasicDeliverEventArgs> EventHandler { get; }
    private RabbitMqConnectionProvider RabbitMq { get; }

    public async Task<ParserSubscribtion> Subscribe(CancellationToken ct = default)
    {
        RerpOpts.ParserInfo(out var domain, out var type);
        await CreateTemporaryResponseListeningQueue(ct);
        ParserSubscribtion subscribtion = new(Guid.NewGuid(), domain, type, DateTime.UtcNow);
        await Publisher.Publish(subscribtion);
        await Tcs.Task;
        return subscribtion;
    }

    private async Task CreateTemporaryResponseListeningQueue(CancellationToken ct)
    {
        Logger?.Information("Creating temporary response listener queue");

        RerpOpts.ResponseQueueInfo(out var queue, out var exchange, out var routingKey);
        object[] logProperties = [queue, exchange, routingKey];

        Logger?.Information(
            """
            Response listener settings info:
            Queue: {queue}
            Exchange: {exchange}
            Routing key: {routingKey}
            """, logProperties);

        CreateChannelOptions channelOpts = new(true, true);
        ResponseQueueChannel = await (await RabbitMq(ct)).CreateChannelAsync(channelOpts, ct);
        await ResponseQueueChannel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: ct);
        await ResponseQueueChannel.QueueDeclareAsync(queue, durable: false, exclusive: false, autoDelete: false, cancellationToken: ct);
        await ResponseQueueChannel.QueueBindAsync(queue, exchange, routingKey, cancellationToken: ct);

        Logger?.Information("Temporary response listener queue created");
        Logger?.Information("Attaching AsyncEventingBasicConsumer for temporary response listener queue");

        Consumer = new AsyncEventingBasicConsumer(ResponseQueueChannel);
        Consumer.ReceivedAsync += EventHandler;
        await ResponseQueueChannel.BasicConsumeAsync(queue, true, Consumer, ct);

        Logger?.Information("Reply queue is ready.");
    }

    private AsyncEventHandler<BasicDeliverEventArgs> Handler()
    {
        return async (_, ea) =>
        {
            Logger?.Information("Received response message. Invoking subscription storage.");
            try
            {
                ParserSubscribtion subscribtion = ParserSubscribtion.FromEventArgs(ea);
                await Storage.SaveSubscription(subscribtion);
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
    }

    private async Task DestroyResponseQueueConsumer()
    {
        RerpOpts.ResponseQueueInfo(out var queue, out var exchange, out var routingKey);
        await ResponseQueueChannel.QueueUnbindAsync(queue, exchange, routingKey);
        await ResponseQueueChannel.QueuePurgeAsync(queue);
        Consumer.ReceivedAsync -= EventHandler;
        await ResponseQueueChannel.DisposeAsync();
    }
}