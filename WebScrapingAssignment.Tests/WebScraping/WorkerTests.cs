

using Microsoft.Extensions.Logging;
using Moq;
using System.Runtime.CompilerServices;
using WebScrapingAssignment.WebScraping;

namespace WebScrapingAssignment.Tests.WebScraping
{
    public class WorkerTests
    {
        private readonly Mock<IHelper> helper = new Mock<IHelper>();
        private readonly Mock<ILogger<Worker>> logger = new Mock<ILogger<Worker>>();

        [Fact]
        public async Task RunAsync_BaseUrlIsEmpty_CheckConstructFileNameAndDownloadAsyncNeverCalled()
        {
            var sut = new Worker(helper.Object, logger.Object);

            await sut.RunAsync("", "destinationFolderName");

            helper.Verify(v => v.ParseTagsForLinks(It.IsAny<HtmlAgilityPack.HtmlDocument>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            helper.Verify(v => v.ConstructFileNameAndDownloadAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RunAsync_DestinationFolderNameIsEmpty_CheckConstructFileNameAndDownloadAsyncNeverCalled()
        {
            var sut = new Worker(helper.Object, logger.Object);

            await sut.RunAsync("baseUrl", "");

            helper.Verify(v => v.ParseTagsForLinks(It.IsAny<HtmlAgilityPack.HtmlDocument>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            helper.Verify(v => v.ConstructFileNameAndDownloadAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
        [Fact]
        public async Task RunAsync_SetUpParseTagsForLinks_CheckCallCountConstructFileNameAndDownloadAsync()
        {
            var htmlDocument = new HtmlAgilityPack.HtmlDocument();
            var pageLinks = new List<string> { "PageLink1", "PageLink2", "PageLink3", "PageLink4" };
            var imageLinks = new List<string> { "ImageLink1", "ImageLink2", "ImageLink3" };
            var scriptLinks = new List<string> { "ScriptLink1", "ScriptLink2" };
            var linkTagLinks = new List<string> { "LinkLink1" };
            helper.Setup(s => s.GetHtmlDocument(It.IsAny<string>())).ReturnsAsync(htmlDocument);
            helper.Setup(s => s.ParseTagsForLinks(It.IsAny<HtmlAgilityPack.HtmlDocument>(), "a", "href")).ReturnsAsync(pageLinks);
            helper.Setup(s => s.ParseTagsForLinks(It.IsAny<HtmlAgilityPack.HtmlDocument>(), "img", "src")).ReturnsAsync(imageLinks);
            helper.Setup(s => s.ParseTagsForLinks(It.IsAny<HtmlAgilityPack.HtmlDocument>(), "script", "src")).ReturnsAsync(scriptLinks);
            helper.Setup(s => s.ParseTagsForLinks(It.IsAny<HtmlAgilityPack.HtmlDocument>(), "link", "href")).ReturnsAsync(linkTagLinks);

            var sut = new Worker(helper.Object, logger.Object);

            await sut.RunAsync("baseUrl", "destinationFolderName");
            helper.Verify(v => v.ParseTagsForLinks(It.IsAny<HtmlAgilityPack.HtmlDocument>(), "a", "href"), Times.Exactly(1));
            helper.Verify(v => v.ParseTagsForLinks(It.IsAny<HtmlAgilityPack.HtmlDocument>(), "img", "src"), Times.Exactly(4));
            helper.Verify(v => v.ParseTagsForLinks(It.IsAny<HtmlAgilityPack.HtmlDocument>(), "script", "src"), Times.Exactly(4));
            helper.Verify(v => v.ParseTagsForLinks(It.IsAny<HtmlAgilityPack.HtmlDocument>(), "link", "href"), Times.Exactly(4));
            helper.Verify(v => v.ConstructFileNameAndDownloadAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(10) );


        }

    }
}
