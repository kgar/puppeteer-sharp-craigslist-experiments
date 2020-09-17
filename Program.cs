using System;
using System.Threading.Tasks;
using PuppeteerSharp;
using PuppeteerSharp.Input;

namespace CraigslistPuppeteerExperiment
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);

            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false
            });

            var page = await browser.NewPageAsync();

            await page.GoToAsync("https://mobile.craigslist.org/", new NavigationOptions
            {
                WaitUntil = new [] {WaitUntilNavigation.Networkidle0}
            });

            var searchTextbox = await page.QuerySelectorAsync("#query");

            await searchTextbox.TypeAsync("nintendo switch");

            await searchTextbox.PressAsync(Key.Enter);

            await page.WaitForNavigationAsync(new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
            });

            var results = await page.QuerySelectorAllAsync(".rows > li");

            foreach (var result in results)
            {
                // title
                var resultTitle = await result
                    .EvaluateFunctionAsync<string>(
                        "li => li.querySelector('.result-title').innerText");
                
                Console.WriteLine($"Title: {resultTitle}");

                // date
                var date = await result
                    .EvaluateFunctionAsync<string>(
                        "li => li.querySelector('.result-date').getAttribute('title')");

                Console.WriteLine($"Date: {date}");

                // $ amount
                var price = await result
                    .EvaluateFunctionAsync<string>(
                        "li => li.querySelector('.result-price').innerText");

                Console.WriteLine($"Price: {price}");

                // location?
                var location = await result
                    .EvaluateFunctionAsync<string>(
                        @"li => {
                            var locationNode = li.querySelector('.result-hood');
                            return locationNode !== null 
                                ? locationNode.innerText 
                                : null;
                        }");

                Console.WriteLine($"Location: {location}");

                // href of the image
                var imageSrc = await result
                    .EvaluateFunctionAsync<string>(
                        @"li => {
                            var img = li.querySelector('.result-image img');
                            return img !== null
                                ? img.getAttribute('src')
                                : null;
                        }");

                Console.WriteLine($"Image SRC: {imageSrc}");

                // url of the result
                var resultUrl = await result
                    .EvaluateFunctionAsync<string>(
                        "li => li.querySelector('.result-title').getAttribute('href')");

                Console.WriteLine($"Result HREF: {resultUrl}");

                Console.WriteLine();
            }


            browser.Dispose();
        }
    }
}
