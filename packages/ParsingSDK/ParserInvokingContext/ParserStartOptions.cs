namespace ParsingSDK.ParserInvokingContext;

public sealed class ParserStartOptions
{
    public string Queue { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
}