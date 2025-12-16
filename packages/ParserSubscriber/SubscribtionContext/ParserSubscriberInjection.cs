using Microsoft.Extensions.DependencyInjection;

namespace ParserSubscriber.SubscribtionContext;

public static class ParserSubscriberInjection
{
    extension(IServiceCollection services)
    {
        public void RegisterParserSubscriber<T>(
            Action<IServiceCollection> rabbitMqProviderConfiguration,
            Action<IServiceCollection> npgSqlProviderConfiguration,
            string schemaName)
            where T : class, IParserSubscriber
        {
            rabbitMqProviderConfiguration(services);
            npgSqlProviderConfiguration(services);
            services.AddTransient<IParserSubscriber, T>();
            services.RegisterSubscriptionStorage(schemaName);
            services.RegisterPublisher();
        }
        
        private void RegisterSubscriptionStorage(string schemaName)
        {
            services.AddSingleton<SubscriptionStorage>(sp =>
            {
                var logger = sp.GetService<Serilog.ILogger>();
                var provider = sp.GetRequiredService<NpgSqlProvider>();
                SubscriptionStorage storage = new(provider, logger);
                storage.SetSchema(schemaName);
                return storage;
            });
        }

        private void RegisterPublisher() => services.AddTransient<ParserSubscriptionPublisher>();
    }

    extension(IServiceProvider services)
    {
        public async Task RunParserSubscription()
        {
            await using AsyncServiceScope scope = services.CreateAsyncScope();
            IParserSubscriber subscriber = scope.ServiceProvider.GetRequiredService<IParserSubscriber>();
            await subscriber.Subscribe();
        }
    }
}