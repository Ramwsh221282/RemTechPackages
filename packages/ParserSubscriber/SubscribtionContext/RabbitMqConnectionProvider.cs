using RabbitMQ.Client;

namespace ParserSubscriber.SubscribtionContext;

public delegate Task<IConnection> RabbitMqConnectionProvider(CancellationToken ct = default);