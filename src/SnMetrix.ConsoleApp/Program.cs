using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SenseNet.Extensions.DependencyInjection;
using SenseNet.IO.Implementations;
using SnMetrix.Client;
using SnMetrix.Client.Plugins;

namespace SnMetrix.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();

            var logger = host.Services.GetService<ILogger<Program>>();

            logger.LogInformation("Building initial structure.");
            
            // build initial structure
            var structureBuilder = host.Services.GetService<InitialStructureBuilder>();
            await structureBuilder.BuildAsync();

            logger.LogInformation("Starting measurement operation.");

            // perform measurement
            var plugin = host.Services.GetService<IMetrixPlugin>();
            await plugin.ExecuteAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder => builder
                    .AddJsonFile("appsettings.json", true, true)
                    .AddUserSecrets<Program>()
                )
                .ConfigureServices((hb, services) => services
                    .AddLogging(logging =>
                    {
                        logging.AddConsole();
                        logging.AddFile("Logs/sn-metrix-{Date}.txt", LogLevel.Trace);
                    })
                    // general sensenet services
                    .AddSenseNetRepository(options =>
                    {
                        hb.Configuration.Bind("InitialImport:RepositoryWriter", options);
                    })
                    .AddContentFlow<FsReader, RepositoryWriter>()
                    // feature-specific services
                    .AddSingleton<InitialStructureBuilder>()
                    .AddSingleton<IMetrixPlugin, FlatContentImporter>()
                    // configure feature-specific options
                    .Configure<InitialStructureBuilderOptions>(hb.Configuration.GetSection("InitialStructure"))
                    .Configure<FsReaderArgs>(hb.Configuration.GetSection("InitialImport:FileSystemReader"))
                    .Configure<RepositoryWriterArgs>(hb.Configuration.GetSection("InitialImport:RepositoryWriter"))
                );
    }
}
