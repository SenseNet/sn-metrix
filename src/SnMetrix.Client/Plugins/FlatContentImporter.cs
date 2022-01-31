using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SenseNet.Client;

namespace SnMetrix.Client.Plugins
{
    public class FlatContentImporter : IMetrixPlugin
    {
        private readonly IServerContextFactory _serverFactory;
        private readonly FlatContentImporterOptions _options;
        private readonly ILogger<FlatContentImporter> _logger;

        public int OperationCount => _options.ContentCount;
        public int MaxDegreeOfParallelism => _options.DegreeOfParallelism;

        public FlatContentImporter(IServerContextFactory serverFactory,
            IOptions<FlatContentImporterOptions> options,
            ILogger<FlatContentImporter> logger)
        {
            _serverFactory = serverFactory;
            _options = options?.Value ?? new FlatContentImporterOptions();
            _logger = logger;
        }

        private int _finishedCount;
        private IProgress<(int, TimeSpan)> _progress;
        private ServerContext _server;

        public async Task ExecuteAsync(IProgress<(int, TimeSpan)> progress)
        {
            _progress = progress;
            _server = await _serverFactory.GetServerAsync();

            var executionOptions = new ExecutionDataflowBlockOptions {MaxDegreeOfParallelism = _options.DegreeOfParallelism};
            var worker = new ActionBlock<int>(Work, executionOptions);

            for (int i = 0; i < _options.ContentCount; i++)
                worker.Post(i);

            worker.Complete();
            worker.Completion.Wait();
        }

        private async Task Work(int index)
        {
            var startTime = DateTime.UtcNow;
            var content = Content.CreateNew(_options.Container,
                "FlatContent", $"{index}-{Guid.NewGuid()}", null, _server);
            await content.SaveAsync();
            var duration = DateTime.UtcNow - startTime;
            _progress?.Report((Interlocked.Increment(ref _finishedCount), duration));
        }
    }
}
