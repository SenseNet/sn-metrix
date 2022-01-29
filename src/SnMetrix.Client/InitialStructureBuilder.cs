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
            var server = await _serverFactory.GetServerAsync();

            // import initial structure
            if (_options.Overwrite)
                await _contentFlow.TransferAsync(new Progress<TransferState>());
            
            // perform manual initial structure building tasks (e.g. duplicating imported content)
            for (var i = 0; i < 10; i++)
            {
                var content = Content.CreateNew("/Root/Content/MetrixWorkspace/flatcontent", "FlatContent",
                    Guid.NewGuid().ToString(), null, server);

                await content.SaveAsync();

                _logger.LogTrace($"Content {content.Path} saved.");
            }
        }
    }
}
