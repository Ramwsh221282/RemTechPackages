using PuppeteerSharp;

namespace ParsingSDK.Parsing;

public static class ProductionBrowserInstantiationMethodImplementation
{
    extension(BrowserInstantiationMethod)
    {
        public static BrowserInstantiationMethod Production => async options =>
        {
            ScrapingBrowserOptions reduced = options.Value;
            if (string.IsNullOrWhiteSpace(reduced.BrowserPath))
                throw new InvalidOperationException("Browser path is empty.");
            if (!reduced.Headless)
                throw new InvalidOperationException("Production browser must be headless.");
            
            LaunchOptions launchOptions = new();
            launchOptions.ApplyFromExternalOptions(reduced);
            return await Puppeteer.LaunchAsync(launchOptions);
        };
    }
}