namespace ParsingSDK.Publishing.BrokerPublishing;

public sealed record BrokerPublishingOptions(
    string Queue,
    string Exchange,
    string RoutingKey,
    string Type
) : IPublishingOptions;