using PuppeteerSharp;

namespace ParsingSDK.Parsing;

public static class PageActions
{
    extension(IPage page)
    {
        public async Task<Maybe<IElementHandle>> GetElementRetriable(string selectorQuery, int retryAmount = 30)
        {
            int currentAttempt = 0;
            while (currentAttempt < retryAmount)
            {
                Maybe<IElementHandle> maybe = await Maybe<IElementHandle>.Resolve(() => page.QuerySelectorAsync(selectorQuery));
                if (maybe.HasValue) return maybe;
                await Task.Delay(TimeSpan.FromSeconds(1));
                currentAttempt++;
            }
                
            return Maybe<IElementHandle>.None();
        }

        public async Task<IElementHandle[]> GetElementsRetriable(string selectorQuery, int retryAmount = 30)
        {
            int currentAttempt = 0;
            while (currentAttempt < retryAmount)
            {
                IElementHandle[] elements = await page.QuerySelectorAllAsync(selectorQuery);
                if (elements.Length > 0) return elements;
                await Task.Delay(TimeSpan.FromSeconds(1));
                currentAttempt++;
            }

            return [];
        }
        
        public async Task ScrollBottom()
        {
            await page.EvaluateExpressionAsync("window.scrollBy(0, document.documentElement.scrollHeight)");
        }

        public async Task ScrollTop()
        {
            await page.EvaluateExpressionAsync("window.scrollTo(0, 0)");
        }
        
        public async Task PerformQuickNavigation(string url, int timeout = 1000)
        {
            NavigationOptions options = new() { Timeout = timeout, WaitUntil = [WaitUntilNavigation.DOMContentLoaded] };
            Task<IResponse> navigationTask = page.WaitForNavigationAsync(options);
            page.GoToAsync(url, options);
            try
            {
                await navigationTask;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Puppeteer navigation error: {ex.Message}");
            }
        }

        public async Task<Maybe<IElementHandle>> ResilientWaitForSelectorWithReturn(string selector, int attempts = 5, int timeout = 5000)
        {
            int waitAttempts = 5;
            int currentAttempt = 0;
            while (currentAttempt < waitAttempts)
            {
                IElementHandle? maybe = await page.WaitForSelectorAsync(selector, new WaitForSelectorOptions() { Timeout = timeout });
                if (maybe != null) return Maybe<IElementHandle>.Some(maybe);
                currentAttempt++;
            }
            return Maybe<IElementHandle>.None();
        }
        
        public async Task ResilientWaitForSelector(string selector, int attempts = 5)
        {
            int waitAttempts = 5;
            int currentAttempt = 0;
            while (currentAttempt < waitAttempts)
            {
                try
                { 
                    await page.WaitForSelectorAsync(selector, new WaitForSelectorOptions { Timeout = 1000 });
                    break;
                }
                catch
                {
                    currentAttempt++;
                }
            }
        }
        
        public async Task PerformNavigation(string url)
        {
            try
            {
                await page.GoToAsync(url, timeout: 0);
            }
            catch(NavigationException)
            {
                Console.WriteLine($"Puppeteer timeout navigation exceeded.");
            }
        }
    }
}