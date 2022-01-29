using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SenseNet.Extensions.DependencyInjection;
using SenseNet.IO.Implementations;
using SnMetrix.Client;

namespace SnMetrix.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();
            
            var structureBuilder = host.Services.GetService<InitialStructureBuilder>();

            await structureBuilder.BuildAsync();
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
                    .AddSenseNetClientTokenStore()
                    .AddContentFlow<FsReader, RepositoryWriter>()
                    // feature-specific services
                    .AddSingleton<IServerContextFactory, ServerContextFactory>()
                    .AddSingleton<InitialStructureBuilder>()
                    // configure feature-specific options
                    .Configure<InitialStructureBuilderOptions>(hb.Configuration.GetSection("InitialStructure"))
                    .Configure<RepositoryOptions>(hb.Configuration.GetSection("InitialImport:RepositoryWriter"))
                    .Configure<FsReaderArgs>(hb.Configuration.GetSection("InitialImport:FileSystemReader"))
                    .Configure<RepositoryWriterArgs>(hb.Configuration.GetSection("InitialImport:RepositoryWriter"))
                );
    }
}
