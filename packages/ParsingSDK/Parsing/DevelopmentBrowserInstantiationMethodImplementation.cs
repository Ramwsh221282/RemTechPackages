using PuppeteerSharp;
using PuppeteerSharp.BrowserData;

namespace ParsingSDK.Parsing;

public static class DevelopmentBrowserInstantiationMethodImplementation
{
    extension(BrowserInstantiationMethod)
    {
        public static BrowserInstantiationMethod Development => async options =>
        {
            ScrapingBrowserOptions reduced = options.Value;
            BrowserFetcher fetcher = new();
            if (!HasAvailableBrowsers(fetcher))
                await DownloadBrowser(fetcher);
            return await LaunchBrowser(fetcher, reduced);
        };
    }

    private static bool HasAvailableBrowsers(BrowserFetcher fetcher)
    {
        IEnumerable<InstalledBrowser> browsers = fetcher.GetInstalledBrowsers();
        return browsers.Any();
    }

    private static async Task<IBrowser> LaunchBrowser(BrowserFetcher fetcher, ScrapingBrowserOptions options)
    {
        IEnumerable<InstalledBrowser> browsers = fetcher.GetInstalledBrowsers();
        InstalledBrowser browser = browsers.First();
        
        ScrapingBrowserOptions withPath = new()
        {
            BrowserPath = browser.GetExecutablePath(),
            Headless = options.Headless,
            DevelopmentMode = options.DevelopmentMode
        };
        
        LaunchOptions launchOptions = new();
        launchOptions.ApplyFromExternalOptions(withPath);
        return await Puppeteer.LaunchAsync(launchOptions);
    }
    
    private static async Task DownloadBrowser(BrowserFetcher fetcher)
    {
        fetcher.Browser = SupportedBrowser.Chromium;
        await fetcher.DownloadAsync();
    }
}