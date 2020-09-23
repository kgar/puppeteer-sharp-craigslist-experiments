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

            var searchResults = await page.EvaluateExpressionAsync<List<SearchResult>>(@"
                (function () {
                  let resultNodes = document.querySelectorAll('.rows > *');
                  let searchResults = [];

                  for (let i = 0; i < resultNodes.length; i++) {
                    let node = resultNodes[i];

                    let shouldStop = node.classList.contains('nearby');
                    if (shouldStop) {
                      break;
                    }

                    let title = node.querySelector('.result-title').innerText;

                    let isValid = title.toLowerCase().indexOf('nintendo switch') >= 0;
                    if (!isValid) {
                      continue;
                    }

                    let date = node.querySelector('.result-date').getAttribute('title');
                    let amount = node.querySelector('.result-price').innerText;

                    let locationNode = node.querySelector('.result-hood');
                    let location = locationNode !== null ? locationNode.innerText : null;

                    let imgNode = node.querySelector('.result-image img');
                    let imageSrc = imgNode !== null ? imgNode.getAttribute('src') : null;

                    let resultUrl = node.querySelector('.result-title').getAttribute('href');

                    let searchResult = {
                      title: title,
                      date: date,
                      amount: amount,
                      location: location,
                      imageSrc: imageSrc,
                      resultUrl: resultUrl,
                      newFieldHere: 'Sabotage!'
                    };

                    searchResults.push(searchResult);
                  }

                  return searchResults;
                })();

            ");

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
