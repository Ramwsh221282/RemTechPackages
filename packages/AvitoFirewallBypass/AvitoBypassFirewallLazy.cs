using PuppeteerSharp;

namespace AvitoFirewallBypass;

public sealed class AvitoBypassFirewallLazy(IPage page, IAvitoBypassFirewall origin) : IAvitoBypassFirewall
{
    public async Task<bool> Bypass()
    {
        if (await FireWallExists())
        {
            await origin.Bypass();
            return await FireWallExists();
        }
        
        return true;
    }

    private async Task<bool> FireWallExists()
    {
        const string javaScript = @"
        () => new Promise(resolve => {
            setTimeout(() => {
                const selector = document.querySelector('div[class=""firewall-container""]');
                resolve(selector === null);
            }, 2000);
        })";

        bool exists = await page.EvaluateFunctionAsync<bool>(javaScript);
        return !exists;
    }
}