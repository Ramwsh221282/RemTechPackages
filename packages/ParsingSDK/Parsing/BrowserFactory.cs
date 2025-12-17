using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace ParsingSDK.Parsing;

public sealed class BrowserFactory(IOptions<ScrapingBrowserOptions> options)
{
    private ScrapingBrowserOptions External { get; } = options.Value;
    
    public async Task<IBrowser> ProvideBrowser()
    {
        LaunchOptions launchOptions = new LaunchOptions();
        launchOptions.ConfigureBasic();
        launchOptions.ApplyFromExternalOptions(External);
        
        try
        {
            return await Instantiate(launchOptions);
        }
        catch
        {
            await LoadBrowser();
        }
        
        return await Instantiate(launchOptions);
    }

    private async Task<IBrowser> Instantiate(LaunchOptions options)
    {
        IBrowser browser = await Puppeteer.LaunchAsync(options);
        browser.DefaultWaitForTimeout = 0;
        return browser;
    }
    
    private async Task LoadBrowser()
    {
        BrowserFetcher fetcher = new(SupportedBrowser.Chromium);
        await fetcher.DownloadAsync();
    }
}