using Microsoft.Extensions.DependencyInjection;

namespace AvitoFirewallBypass;

public static class AvitoFirewallBypassInjection
{
    extension(IServiceCollection services)
    {
        public void RegisterAvitoFirewallBypass()
        {
            services.AddSingleton<AvitoBypassFactory>();
        }
    }
}