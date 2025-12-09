namespace ParsingSDK.Publishing.TextPublishing;

public sealed record TextFileSaveOptions(
    string FilePath
) : IPublishingOptions;