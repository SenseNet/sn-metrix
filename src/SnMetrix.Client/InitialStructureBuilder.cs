using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SenseNet.Client;
using SenseNet.IO;

namespace SnMetrix.Client
{
    public interface IStructureBuilder
    {
        Task BuildAsync();
    }

    public class InitialStructureBuilder : IStructureBuilder
    {
        private readonly IServerContextFactory _serverFactory;
        private readonly IContentFlow _contentFlow;
        private readonly ILogger<InitialStructureBuilder> _logger;
        private readonly InitialStructureBuilderOptions _options;

        public InitialStructureBuilder(IServerContextFactory serverFactory,
            IContentFlow contentFlow,
            IOptions<InitialStructureBuilderOptions> options,
            ILogger<InitialStructureBuilder> logger)
        {
            _serverFactory = serverFactory;
            _contentFlow = contentFlow;
            _logger = logger;
            _options = options.Value;
        }

        public async Task BuildAsync()
        {
            // import initial structure
            if (_options.Overwrite)
                await _contentFlow.TransferAsync(new Progress<TransferState>());
            
            // perform additional structure building tasks (e.g. duplicating imported content)
            //var server = _serverFactory.GetServerAsync().ConfigureAwait(false);

            _logger.LogInformation("Building initial structure is complete.");
        }
    }
}
