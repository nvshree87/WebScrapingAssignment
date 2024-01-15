using Castle.Core.Logging;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using NuGet.Frameworks;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using WebScrapingAssignment.WebScraping;

namespace WebScrapingAssignment.Tests.WebScraping
{
    public class HelperTests
    {
        private readonly Mock<IHttpClientFactory> httpClientFactory = new Mock<IHttpClientFactory>();
        private readonly Mock<ILogger<Helper>> logger = new Mock<ILogger<Helper>>();
        private readonly Mock<HttpMessageHandler> httpMessageHandler = new Mock<HttpMessageHandler>();

        [Fact]
        public void ExtractPathAndFileName_InputUrl_PathAndFileNameReturned()
        {
            var sut = new Helper(httpClientFactory.Object, logger.Object);

            var (resultPath, resultFileName) = sut.ExtractPathAndFileName("""Catalogue/Category/Travel/index.html""");

            Assert.NotNull(resultPath);
            Assert.NotNull(resultFileName);
            Assert.Equal("""Catalogue/Category/Travel""", resultPath);
            Assert.Equal("index.html", resultFileName);
        }

        [Fact]
        public void ExtractPathAndFileName_emptyUrl_ThrowsArgumentException()
        {
            var sut = new Helper(httpClientFactory.Object, logger.Object);

            Assert.Throws<ArgumentNullException>(() => sut.ExtractPathAndFileName(""));
        }

        [Fact]
        public async Task GetHtmlDocument_SetupHtmlData_TestLoadedHtmlData()
        {
            var htmlData = "<html>TestData</html>";

            HttpClient client = new HttpClient(httpMessageHandler.Object);
            client.BaseAddress = new Uri("https://localhost");
            httpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var mockedProtected = httpMessageHandler.Protected();
            mockedProtected.Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            ).ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(htmlData)
            });

            var sut = new Helper(httpClientFactory.Object, logger.Object);

            var result = await sut.GetHtmlDocument("url");

            Assert.NotNull(result);
            Assert.Equal(htmlData, result.DocumentNode.InnerHtml);
        }

        [Theory]
        [InlineData("a", "href", 7)]
        [InlineData("img", "src", 3)]
        [InlineData("script", "src", 2)]
        [InlineData("link", "href", 1)]
        public async Task ParseTagsForLinks_InputTagNameAndAttributeToFetchLinks_ReturnsExpectedLinksCount(string tagName, string attributeName, int expectedLinksCount)
        {
            var htmlData = GetHtmlData();
            
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlData);

            var sut = new Helper(httpClientFactory.Object, logger.Object);

            var result = await sut.ParseTagsForLinks(htmlDoc, tagName, attributeName);

            Assert.Equal(expectedLinksCount, result.Count());
        }

        [Fact]
        public async Task ConstructFileNameAndDownloadAsync_SetUpToDownloadFileToDestination_CheckDestinationFileCreated()
        {
            var destinationFile = Path.Combine(Directory.GetCurrentDirectory(),
                "destination/WebScraping/source/test.txt");
            if (File.Exists(destinationFile))
                File.Delete(destinationFile);

            HttpClient client = new HttpClient(httpMessageHandler.Object);
            client.BaseAddress = new Uri("https://localhost");
            httpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var mockedProtected = httpMessageHandler.Protected();
            mockedProtected.Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            ).ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("")
            });

            var sut = new Helper(httpClientFactory.Object, logger.Object);

            await sut.ConstructFileNameAndDownloadAsync("WebScraping/source/test.txt", "destination");

            logger.Verify(v => v.Log(It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString().Contains("Downloading")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
            Assert.True(File.Exists(destinationFile));
        }

        [Fact]
        public async Task ConstructFileNameAndDownloadAsync_DestinationFileExists_ResturnsByOnlyLogging()
        {
            var destinationFile = Path.Combine(Directory.GetCurrentDirectory(),
                "destination/WebScraping/source/test.txt");
            if (!File.Exists(destinationFile))
                File.Create(destinationFile);

            HttpClient client = new HttpClient(httpMessageHandler.Object);
            client.BaseAddress = new Uri("https://localhost");
            httpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var mockedProtected = httpMessageHandler.Protected();
            mockedProtected.Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            ).ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("")
            });

            var sut = new Helper(httpClientFactory.Object, logger.Object);

            await sut.ConstructFileNameAndDownloadAsync("WebScraping/source/test.txt", "destination");

            logger.Verify(v => v.Log(It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString().Contains("Skipping")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        private string GetHtmlData()
        {
            return "<!DOCTYPE html>  \r\n\t<head>\r\n\t\t<link rel=\"shortcut icon\" href=\"static/oscar/favicon.ico\" />\r\n\t" +
                "</head>\r\n\t<a href=\"index.html\">All Products</a>\r\n\t<body id=\"default\" class=\"default\">\r\n\t\t" +
                "<div class=\"side_categories\">\r\n\t\t\t<li><a href=\"catalogue/category/books/religion_12/index.html\">Religion</a>" +
                "</li>\r\n\t\t\t<li><a href=\"catalogue/category/books/nonfiction_13/index.html\">Nonfiction</a>" +
                "</li>\r\n\t\t\t<li><a href=\"catalogue/category/books/fiction_14/index.html\">Fiction</a>" +
                "</li>\r\n\t\t</div>\r\n\t\t" +
                "<div class=\"image_container\">\r\n\t\t\t" +
                "<a href=\"catalogue/a-light-in-the-attic_1000/index.html\">" +
                "<img src=\"media/cache/2c/da/2cdad67c44b002e7ead0cc35693c0e8b.jpg\" alt=\"A Light in the Attic\" class=\"thumbnail\"></a>\r\n\t\t\t" +
                "<a href=\"catalogue/soumission_998/index.html\">" +
                "<img src=\"media/cache/3e/ef/3eef99c9d9adef34639f510662022830.jpg\" alt=\"Soumission\" class=\"thumbnail\"></a>\r\n\t\t\t" +
                "<a href=\"catalogue/sharp-objects_997/index.html\">" +
                "<img src=\"media/cache/32/51/3251cf3a3412f53f339e42cac2134093.jpg\" alt=\"Sharp Objects\" class=\"thumbnail\"></a>\r\n\t\t</div>\r\n\t\t" +
                "<script type=\"text/javascript\" src=\"static/oscar/js/bootstrap3/bootstrap.min.js\"></script>\r\n\t\t" +
                "<script src=\"static/oscar/js/oscar/ui.js\" type=\"text/javascript\" charset=\"utf-8\"></script>\r\n\t</body>\r\n</html>";
        }
    }
}