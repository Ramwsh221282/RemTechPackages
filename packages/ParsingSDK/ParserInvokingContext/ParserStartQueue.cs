using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ParsingSDK.ParserInvokingContext;

public sealed class ParserStartQueue(
    IOptions<ParserStartOptions> options, 
    ParserStartQueueRabbitMqProvide connectionProvide,
    ParserStartQueueHandle handle,
    Serilog.ILogger? logger) : BackgroundService
{
    private ParserStartOptions Options { get; } = options.Value;
    private Serilog.ILogger? Logger { get; } = logger?.ForContext<ParserStartQueue>();
    private IChannel Channel { get; set; } = null!;
    
    private AsyncEventingBasicConsumer Consumer { get; set; } = null!;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger?.Information("Creating parser start queue");
        
        Logger?.Information("""
                            Parser start queue options info:
                            Queue: {Queue}
                            Exchange: {Exchange}
                            RoutingKey: {RoutingKey}
                            """,
            Options.Queue,
            Options.Exchange,
            Options.RoutingKey);
        
        ValidateParserStartOptions();
        IConnection connection = await connectionProvide(stoppingToken);
        Channel = await CreateChannel(connection, stoppingToken);
        await ConfigureExchange(Channel, stoppingToken);
        await ConfigureQueue(Channel, stoppingToken);
        await BindQueueToExchange(Channel, stoppingToken);
        Consumer = CreateConsumer();
        Logger?.Information("Parser start queue, exchange configured.");
        Logger?.Information("Starting consuming.");
        await Channel.BasicConsumeAsync(
            queue: Options.Queue, 
            autoAck: false, 
            consumer: Consumer,
            cancellationToken: stoppingToken);
    }

    private void ValidateParserStartOptions()
    {
        if (string.IsNullOrWhiteSpace(Options.Queue))
        {
            Logger?.Error("Queue is empty.");
            throw new Exception("Queue is empty.");
        }

        if (string.IsNullOrWhiteSpace(Options.Exchange))
        {
            Logger?.Error("Exchange is empty.");
            throw new Exception("Exchange is empty.");
        }

        if (string.IsNullOrWhiteSpace(Options.RoutingKey))
        {
            Logger?.Error("Routing key is empty.");
            throw new Exception("Routing key is empty.");
        }
    }
    
    private async Task<IChannel> CreateChannel(IConnection connection, CancellationToken ct)
    {
        CreateChannelOptions options = new(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true
        );
        
        return await connection.CreateChannelAsync(options, ct);
    }

    private async Task ConfigureExchange(IChannel channel, CancellationToken ct)
    {
        await channel.ExchangeDeclareAsync(
            exchange: Options.Exchange,
            type: "topic",
            durable: true,
            autoDelete: false,
            cancellationToken: ct
        );
    }

    private async Task ConfigureQueue(IChannel channel, CancellationToken ct)
    {
        await channel.QueueDeclareAsync(
            queue: Options.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: ct
        );
    }

    private async Task BindQueueToExchange(IChannel channel, CancellationToken ct)
    {
        await channel.QueueBindAsync(
            queue: Options.Queue,
            exchange: Options.Exchange,
            routingKey: Options.RoutingKey,
            cancellationToken: ct
        );
    }

    private AsyncEventingBasicConsumer CreateConsumer()
    {
        AsyncEventingBasicConsumer consumer = new(Channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            Logger?.Information("Received message to start parsing.");
            try
            {
                await handle(ea);
                Logger?.Information("Start parsing message handled.");
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Error handling message.");
            }
            finally
            {
                await Channel.BasicAckAsync(ea.DeliveryTag, false);
            }
        };
        return consumer;
    }

    private async Task StartConsuming(IChannel channel, AsyncEventingBasicConsumer consumer, CancellationToken ct)
    {
        await channel.BasicConsumeAsync(
            queue: Options.Queue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: ct
        );
    }
}