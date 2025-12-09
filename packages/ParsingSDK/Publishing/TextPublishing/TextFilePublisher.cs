namespace ParsingSDK.Publishing.TextPublishing;

public sealed class TextFilePublisher : IPublisher<TextFileSaveOptions>
{
    public async Task Publish(string content, TextFileSaveOptions options, CancellationToken ct = default)
    {
        using StreamWriter writer = File.CreateText(options.FilePath);
        await writer.WriteLineAsync(content);
        await writer.FlushAsync(ct);
    }
}