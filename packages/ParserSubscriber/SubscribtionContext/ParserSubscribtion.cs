namespace ParserSubscriber.SubscribtionContext;

public sealed record ParserSubscribtion(
    Guid Id,
    string Domain,
    string Type,
    DateTime Subscribed
);