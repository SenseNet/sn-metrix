using System;
using System.Collections.Generic;
using System.Linq;
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
            var times = new List<TimeSpan>();
            var progress = new Progress<(int value, TimeSpan duration)>(progressValue =>
            {
                Console.Write(" {0}/{1}  \r", progressValue.value, plugin.OperationCount);
                times.Add(progressValue.duration);
            });

            var start = DateTime.UtcNow;
            await plugin.ExecuteAsync(progress);
            var executiontime = DateTime.UtcNow - start;
            var cps = 1.0d * plugin.OperationCount * TimeSpan.TicksPerSecond / executiontime.Ticks;

            var avgTicks = times.Select(x=>x.Ticks).Sum() / times.Count;
            var avgTime = TimeSpan.FromTicks(avgTicks);
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine($"Content imported:            {plugin.OperationCount}");
            Console.WriteLine($"Max degree of parallelism:   {plugin.MaxDegreeOfParallelism}");
            Console.WriteLine($"Execution time (sec):        {executiontime.TotalSeconds}");
            Console.WriteLine($"CPS:                         {cps}");
            Console.WriteLine($"Average response time (sec): {avgTime.TotalSeconds}");
            Console.WriteLine("-------------------------------------------");

            logger.LogInformation("Measurement operation finished.");
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
                    .AddSingleton<IMetrixPlugin, FlatContentImporter>()
                    // configure feature-specific options
                    .Configure<InitialStructureBuilderOptions>(hb.Configuration.GetSection("InitialStructure"))
                    .Configure<FlatContentImporterOptions>(hb.Configuration.GetSection("Plugins:FlatContentImporter"))
                    .Configure<RepositoryOptions>(hb.Configuration.GetSection("InitialImport:RepositoryWriter"))
                    .Configure<FsReaderArgs>(hb.Configuration.GetSection("InitialImport:FileSystemReader"))
                    .Configure<RepositoryWriterArgs>(hb.Configuration.GetSection("InitialImport:RepositoryWriter"))
                );
    }
}
