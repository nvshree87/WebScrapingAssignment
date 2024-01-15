using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace WebScrapingAssignment.WebScraping
{
    public class Worker 
    {
        private readonly IHelper helper;
        private readonly ILogger<Worker> logger;

        private static ConcurrentDictionary<string, string> _processedFilesCached = new();

        public Worker(IHelper helper, ILogger<Worker> logger)
        {
            this.helper = helper;
            this.logger = logger;
        }

        public async Task RunAsync(string baseUrl, string destinationFolderName)
        {
            List<Task> taskList = new List<Task>();

            if(string.IsNullOrWhiteSpace(baseUrl)) 
            {
                logger.LogError("Web Scraping Cannot be Started as baseUrl is not provided");
                return; 
            }

            if( string.IsNullOrWhiteSpace(destinationFolderName))
            {
                logger.LogError($"Web Scraping Cannot be Started for {baseUrl} as destinationFolderName is not provided");
                return;
            }

            logger.LogInformation("Web Scraping Started for the Url: {url}", baseUrl);

            var htmlDocument = await helper.GetHtmlDocument("");

            var pageLinks = await helper.ParseTagsForLinks(htmlDocument, "a", "href");

            taskList.Add(ExtractFileNamesAndDownloadAsync(pageLinks, destinationFolderName));

            foreach (var pageLink in pageLinks)
            {
                var bookHtmlDocument = await helper.GetHtmlDocument(pageLink);

                var linkList = await helper.ParseTagsForLinks(bookHtmlDocument, "link", "href");
                var imageList = await helper.ParseTagsForLinks(bookHtmlDocument, "img", "src");
                var scriptList = await helper.ParseTagsForLinks(bookHtmlDocument, "script", "src");

                taskList.Add(ExtractFileNamesAndDownloadAsync(linkList, destinationFolderName));
                taskList.Add(ExtractFileNamesAndDownloadAsync(imageList, destinationFolderName));
                taskList.Add(ExtractFileNamesAndDownloadAsync(scriptList, destinationFolderName));
            }

            Task.WaitAll(taskList.ToArray());

            logger.LogInformation("Web Scraping Completed Successfully for the Url: {url}", baseUrl);
        }

        private async Task ExtractFileNamesAndDownloadAsync(List<string> filesList, string destinationFolderName) 
        { 
            foreach(var file in filesList)
            {
                if (_processedFilesCached.ContainsKey(file))
                {
                    logger.LogInformation("File already Downloaded {file}", file);
                    continue;
                }

                await helper.ConstructFileNameAndDownloadAsync(file, destinationFolderName);

                _processedFilesCached.TryAdd(file, "");
            }
        }
    }
}
