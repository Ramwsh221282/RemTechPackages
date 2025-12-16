using ParserSubscriber.SubscribtionContext;

namespace ParserSubscriber;

public interface IParserSubscriber
{
    Task<ParserSubscribtion> Subscribe(CancellationToken ct = default);
}