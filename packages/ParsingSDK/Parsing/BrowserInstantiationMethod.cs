using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace ParsingSDK.Parsing;

public delegate Task<IBrowser> BrowserInstantiationMethod(IOptions<ScrapingBrowserOptions> options);