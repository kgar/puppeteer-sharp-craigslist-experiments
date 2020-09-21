using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

            var resultNodes = await page.QuerySelectorAllAsync(".rows > *");

            var searchResults = new List<SearchResult>();

            foreach (var result in resultNodes)
            {
                var shouldStop = await result
                    .EvaluateFunctionAsync<bool>("n => n.classList.contains('nearby')");

                if (shouldStop)
                {
                    break;
                }

                var title = await result
                    .EvaluateFunctionAsync<string>(
                        "li => li.querySelector('.result-title').innerText");

                var isValid = title.Contains("nintendo switch", StringComparison.OrdinalIgnoreCase);

                if (!isValid)
                {
                    continue;
                }

                var searchResult = new SearchResult();

                searchResult.Title = title;
                
                searchResult.Date = await result
                    .EvaluateFunctionAsync<string>(
                        "li => li.querySelector('.result-date').getAttribute('title')");

                searchResult.Amount = await result
                    .EvaluateFunctionAsync<string>(
                        "li => li.querySelector('.result-price').innerText");

                searchResult.Location = await result
                    .EvaluateFunctionAsync<string>(
                        @"li => {
                            var locationNode = li.querySelector('.result-hood');
                            return locationNode !== null 
                                ? locationNode.innerText 
                                : null;
                        }");

                searchResult.ImageSrc = await result
                    .EvaluateFunctionAsync<string>(
                        @"li => {
                            var img = li.querySelector('.result-image img');
                            return img !== null
                                ? img.getAttribute('src')
                                : null;
                        }");

                searchResult.ResultUrl = await result
                    .EvaluateFunctionAsync<string>(
                        "li => li.querySelector('.result-title').getAttribute('href')");

                searchResults.Add(searchResult);
            }

            var serializedResults = JsonConvert.SerializeObject(searchResults, Formatting.Indented);

            Console.WriteLine(serializedResults);
            Console.WriteLine($"Total results: {searchResults.Count}");

            var originalWidth = page.Viewport.Width;

            await SetViewportWidth(page, 1920);

            var screenshotOptions = new ScreenshotOptions
            {
                FullPage = true
            };
            await page.ScreenshotAsync("hello-puppeteer-gang.jpeg", screenshotOptions);

            await SetViewportWidth(page, originalWidth);

            browser.Dispose();
        }

        private static async Task SetViewportWidth(Page page, int width)
        {
            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = width
            });
        }
    }
}
