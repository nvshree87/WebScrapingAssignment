using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebScrapingAssignment;
using WebScrapingAssignment.WebScraping;


var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true, true)
    .Build();

var baseUrl = config.GetSection("WebScraping:BaseUrl").Value;
var destinationFolderName = config.GetSection("WebScraping:DestinationFolderName").Value;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddHttpClient(nameof(Helper), client =>
        {
            client.BaseAddress = new Uri(baseUrl);

        });
        services.AddScoped<IHelper, Helper>();
        services.AddSingleton<Worker>();
    }).Build();

using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    await services.GetRequiredService<Worker>().RunAsync(baseUrl, destinationFolderName);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}


