using Microsoft.Extensions.DependencyInjection;
using ParsingSDK.Parsing;

namespace ParsingSDK;

public static class ParsingDependencyInjection
{
    extension(IServiceCollection services)
    {
        public void RegisterParserDependencies(Action<IServiceCollection>? scrapingOptionsConfiguration = null)
        {
            scrapingOptionsConfiguration?.Invoke(services);
            services.AddBrowserFactory();
        }
        
        public void AddBrowserFactory()
        {
            services.AddSingleton<BrowserFactory>();
        }
    }
}