using Microsoft.Extensions.DependencyInjection;
using ParsingSDK.Publishing.BrokerPublishing;
using ParsingSDK.Publishing.TextPublishing;

namespace ParsingSDK.Publishing;

public static class PublishingDependencyInjection
{
    extension(IServiceCollection services)
    {
        public void RegisterTextPublishing()
        {
            services.AddTransient<IPublisher<TextFileSaveOptions>, TextFilePublisher>();
        }

        public void RegisterBrokerPublishing()
        {
            services.AddTransient<IPublisher<BrokerPublishingOptions>, BrokerPublisher>();
        }
    }
}