using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace myApp.Modules
{
    public class PdfBase64Generator
    {
        private static IBrowser _browser;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public static async Task Initialize()
        {
            await new BrowserFetcher().DownloadAsync();
            _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            });
        }

        public static async Task<string> ConvertHtmlToPdfBase64(string html)
        {
            if (_browser == null)
            {
                await Initialize();
            }

            await _semaphore.WaitAsync();
            try
            {
                using var page = await _browser.NewPageAsync();
                await page.SetContentAsync(html, new NavigationOptions
                {
                    WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
                });

                var pdfBytes = await page.PdfDataAsync(new PdfOptions
                {
                    Format = PaperFormat.A4,
                    PrintBackground = true
                });

                return Convert.ToBase64String(pdfBytes);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async Task Shutdown()
        {
            if (_browser != null)
            {
                await _browser.CloseAsync();
                _browser.Dispose();
                _browser = null;
            }
        }
    }
}
