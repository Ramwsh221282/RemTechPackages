using Microsoft.Extensions.DependencyInjection;

namespace ParsingSDK.TextProcessing;

public static class TextProcessingInjection
{
    extension(IServiceCollection services)
    {
        public void RegisterTextTransformerBuilder()
        {
            services.AddTransient<TextTransformerBuilder>();
        }
    }
}