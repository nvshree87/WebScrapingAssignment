using HtmlAgilityPack;

namespace WebScrapingAssignment.WebScraping
{
    public interface IHelper
    {
        Task ConstructFileNameAndDownloadAsync(string filePath, string destinationFolderName);
        Task<HtmlDocument> GetHtmlDocument(string baseUrl);
        Task<List<string>> ParseTagsForLinks(HtmlDocument doc, string tagName, string attributeName);
    }
}
