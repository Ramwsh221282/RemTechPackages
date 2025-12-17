using PuppeteerSharp;

namespace ParsingSDK;

public static class LaunchOptionsConfigurationImplementation
{
    extension(LaunchOptions options)
    {
        public void ApplyFromExternalOptions(ScrapingBrowserOptions externalOptions)
        {
            options.Headless = externalOptions.Headless;
            if (!string.IsNullOrEmpty(externalOptions.BrowserPath))
                options.ExecutablePath = externalOptions.BrowserPath;
        }
        
        public void ConfigureBasic()
        {
            options.Args =
            [
                "--no-sandbox",
                "--disable-gpu",
                "--disable-dev-shm-usage",
                "--no-zygote",
            ];
            options.UserDataDir = null;
        }
    }
}