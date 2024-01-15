using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace WebScrapingAssignment.WebScraping
{
    public class Helper : IHelper
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<Helper> logger;

        public Helper(IHttpClientFactory httpClientFactory, ILogger<Helper> logger)
        {
            this.httpClient = httpClientFactory.CreateClient(nameof(Helper));
            this.logger = logger;
        }

        public async Task<HtmlDocument> GetHtmlDocument(string url)
        {
            var htmlData = await httpClient.GetStringAsync(url);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlData);

            return htmlDocument;
        }

        public async Task ConstructFileNameAndDownloadAsync(string filePath, string destinationFolderName)
        {
            var (sourceRelativePath, fileName) = ExtractPathAndFileName(filePath);

            var destinationFilePath = Path.Combine(Directory.GetCurrentDirectory(), 
                destinationFolderName, sourceRelativePath);
            var destinationFile = Path.Combine(destinationFilePath, fileName);
            var sourceFile = Path.Combine(sourceRelativePath ?? "", fileName);

            await DownloadAsync(sourceFile, destinationFilePath, destinationFile);
        }

        public async Task<List<string>> ParseTagsForLinks(HtmlDocument doc, string tagName, string attributeName)
        {
            List<string> links = new List<string>();
            var linkTags = doc.DocumentNode.Descendants(tagName);

            foreach (var tag in linkTags)
            {
                var attributeValue = tag.Attributes[attributeName]?.Value.Replace("../", "");

                if (attributeValue == null || tagName == "script" && attributeValue.Contains("http"))
                    continue; //skipping the tag if it has no input src or href mentioned or its
                              //loaded through internet when tag is script

                links.Add(attributeValue);
            }

            return links;
        }

        internal (string path, string fileName) ExtractPathAndFileName(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var splitHref = filePath.Split("/");
            var path = string.Join("/", splitHref.Take(splitHref.Length - 1));
            var fileName = filePath.Split('/').Last();

            return (path, fileName);
        }

        private async Task DownloadAsync(string sourceFile, string destinationFilePath, string destinationFile)
        {
            if (File.Exists(destinationFile))
            {
                logger.LogInformation($"Skipping Download, file already exists : {sourceFile}");
                return;
            }

            if (!Directory.Exists(destinationFilePath))
                Directory.CreateDirectory(destinationFilePath);

            logger.LogInformation($"Downloading file : {sourceFile}");

            var response = await httpClient.GetAsync(sourceFile);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("File not found");
                return;
            }

            var stream = await response.Content.ReadAsStreamAsync();
            var fileInfo = new FileInfo(destinationFile);
            using (var fileStream = fileInfo.OpenWrite())
            {
                await stream.CopyToAsync(fileStream);
            }
        }
    }
}
