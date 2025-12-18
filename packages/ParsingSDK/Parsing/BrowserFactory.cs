using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace ParsingSDK.Parsing;

public sealed class BrowserFactory(IOptions<ScrapingBrowserOptions> options)
{
    private ScrapingBrowserOptions External { get; } = options.Value;
    
    public async Task<IBrowser> ProvideBrowser()
    {
        BrowserInstantiationMethod method = BrowserInstantiationMethodRouter.ResolveMethodByOptions(options);
        return await method(options);
    }
    
    public async Task LoadBrowser()
    {
        BrowserFetcherOptions options = new() { Browser = SupportedBrowser.Chromium };
        BrowserFetcher fetcher = new(options);
        await fetcher.DownloadAsync();
    }
    
    private async Task<IBrowser> Instantiate(LaunchOptions options)
    {
        IBrowser browser = await Puppeteer.LaunchAsync(options);
        browser.DefaultWaitForTimeout = 0;
        return browser;
    }
}