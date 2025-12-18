namespace ParsingSDK;

public sealed class ScrapingBrowserOptions
{
    public bool DevelopmentMode { get; set; } = false;
    public string BrowserPath { get; set; } = string.Empty;
    public bool Headless { get; set; } = false;
}