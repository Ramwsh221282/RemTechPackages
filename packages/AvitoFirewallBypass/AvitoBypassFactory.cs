using PuppeteerSharp;

namespace AvitoFirewallBypass;

public sealed class AvitoBypassFactory
{
    public IAvitoBypassFirewall Create(IPage page)
    {
        return new AvitoByPassFirewallWithRetry(new AvitoBypassFirewallLazy(page, new AvitoBypassFirewall(page)));
    }
}