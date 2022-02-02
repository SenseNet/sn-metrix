using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SenseNet.Client;
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

            // build initial structure
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var factory = host.Services.GetRequiredService<IServerContextFactory>();
            var structureBuilder = host.Services.GetRequiredService<InitialStructureBuilder>();
            await EnsureInitialStructureAsync(structureBuilder, logger, factory);

            // perform measurement
            var plugin = host.Services.GetRequiredService<IMetrixPlugin>();
            var times = new List<TimeSpan>();
            var progress = new Progress<(int value, TimeSpan duration)>(progressValue =>
            {
                Console.Write(" Progress: {0}/{1}  \r", progressValue.value, plugin.OperationCount);
                times.Add(progressValue.duration);
            });

            logger.LogInformation("Preparing measurement operation.");
            await plugin.PrepareAsync();

            logger.LogInformation("Starting measurement operation.");
            var start = DateTime.UtcNow;
            await plugin.ExecuteAsync(progress);
            var executionTime = DateTime.UtcNow - start;
            var cps = 1.0d * plugin.OperationCount * TimeSpan.TicksPerSecond / executionTime.Ticks;

            var avgTicks = times.Select(x=>x.Ticks).Sum() / times.Count;
            var avgTime = TimeSpan.FromTicks(avgTicks);
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("        MEASUREMENT RESULTS        ");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine($"Content imported:            {plugin.OperationCount}");
            Console.WriteLine($"Max degree of parallelism:   {plugin.MaxDegreeOfParallelism}");
            Console.WriteLine($"Execution time (sec):        {executionTime.TotalSeconds:F2}");
            Console.WriteLine($"CPS:                         {cps:F2}");
            Console.WriteLine($"Average response time (sec): {avgTime.TotalSeconds:F4}");
            Console.WriteLine("-----------------------------------");

            logger.LogInformation("Cleaning up measurement operation.");
            await plugin.CleanupAsync();

            logger.LogInformation("Measurement operation finished.");
        }
        private static async Task EnsureInitialStructureAsync(InitialStructureBuilder builder, ILogger logger, IServerContextFactory factory)
        {
            var server = await factory.GetServerAsync();

            if ((await Content.QueryForAdminAsync("+Type:ContentType +Name:FlatContent",
                    new[] { "Id", "Type", "Path" }, server: server).ConfigureAwait(false)).Any())
                return;

            logger.LogInformation("Building initial structure.");
            await builder.BuildAsync();
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
                    .Configure<FlatContentImporterOptions>(hb.Configuration.GetSection("Plugins:FlatContentImporter"))
                    .Configure<FsReaderArgs>(hb.Configuration.GetSection("InitialImport:FileSystemReader"))
                    .Configure<RepositoryWriterArgs>(hb.Configuration.GetSection("InitialImport:RepositoryWriter"))
                );
    }
}
