namespace ParsingSDK.Publishing;

public interface IPublisher
{
    public Task Publish(string content, CancellationToken ct = default);
}

public interface IPublisher<in T> where T : IPublishingOptions
{
    public Task Publish(string content, T options, CancellationToken ct = default);
}