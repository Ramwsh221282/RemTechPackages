using PuppeteerSharp;

namespace ParsingSDK.Parsing;

public sealed class BrowserFactory
{
    public async Task<IBrowser> ProvideBrowser(bool headless = true)
    {
        LaunchOptions options = new LaunchOptions
        {
            Headless = headless,
            Args = 
            [
                "--no-sandbox",
                "--disable-gpu",
                "--disable-dev-shm-usage",
                "--no-zygote",
            ],
            UserDataDir = null
        };
        
        try
        {
            return await Instantiate(options);
        }
        catch
        {
            await LoadBrowser();
        }
        
        return await Instantiate(options);
    }

    private async Task<IBrowser> Instantiate(LaunchOptions options)
    {
        IBrowser browser = await Puppeteer.LaunchAsync(options);
        browser.DefaultWaitForTimeout = 0;
        return browser;
    }
    
    private async Task LoadBrowser()
    {
        await new BrowserFetcher().DownloadAsync();
    }
}