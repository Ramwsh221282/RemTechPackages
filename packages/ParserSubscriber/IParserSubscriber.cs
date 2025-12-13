using ParserSubscriber.Subscribers.RabbitMq;

namespace ParserSubscriber;

public interface IParserSubscriber
{
    Task<ParserSubscribtion> Subscribe(CancellationToken ct = default);
}
