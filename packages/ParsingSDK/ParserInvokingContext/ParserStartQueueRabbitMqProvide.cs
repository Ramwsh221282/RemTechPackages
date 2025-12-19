using RabbitMQ.Client;

namespace ParsingSDK.ParserInvokingContext;

public delegate Task<IConnection> ParserStartQueueRabbitMqProvide(CancellationToken ct);