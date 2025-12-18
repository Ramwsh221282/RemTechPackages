using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

        public void RegisterParserDependencies(Action<ScrapingBrowserOptions> optionsConfiguration)
        {
            ScrapingBrowserOptions options = new ScrapingBrowserOptions();
            optionsConfiguration(options);
            IOptions<ScrapingBrowserOptions> ioptions = Options.Create(options);
            services.AddSingleton(ioptions);
            services.AddSingleton<BrowserFactory>();
        }
        
        public void AddBrowserFactory()
        {
            services.AddSingleton<BrowserFactory>();
        }
    }
}