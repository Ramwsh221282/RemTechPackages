using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ParsingSDK.ParserInvokingContext;

public static class ParserInvokingContextInjection
{
    extension(IServiceCollection services)
    {
        public void RegisterParserStartOptionsByAppsettings()
        {
            services.AddOptions<ParserStartOptions>().BindConfiguration(nameof(ParserStartOptions));
        }

        public void RegisterParserStartOptionsManually(Action<ParserStartOptions> optionsAction)
        {
            ParserStartOptions options = new();
            optionsAction(options);
            IOptions<ParserStartOptions> optionsWrapper = Options.Create(options);
            services.AddSingleton(optionsWrapper);
        }
        
        public void RegisterParserInvokingQueue(Func<IServiceProvider, ParserStartQueueRabbitMqProvide> connectionProvide)
        {
            services.AddSingleton<ParserStartQueueRabbitMqProvide>(sp => connectionProvide(sp));
            services.AddHostedService<ParserStartQueue>();
        }
    }
}