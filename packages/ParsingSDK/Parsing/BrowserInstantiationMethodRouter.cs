using Microsoft.Extensions.Options;

namespace ParsingSDK.Parsing;

public static class BrowserInstantiationMethodRouter
{
    public static BrowserInstantiationMethod ResolveMethodByOptions(IOptions<ScrapingBrowserOptions> options)
    {
        bool isDevelopment = options.Value.DevelopmentMode;
        return isDevelopment switch
        {
            true => BrowserInstantiationMethod.Development,
            false => BrowserInstantiationMethod.Production
        };
    }
}