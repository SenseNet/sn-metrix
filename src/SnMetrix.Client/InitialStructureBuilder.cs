using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SenseNet.Client;

namespace SnMetrix.Client
{
    public interface IStructureBuilder
    {
        Task BuildAsync();
    }

    public class InitialStructureBuilder : IStructureBuilder
    {
        private readonly IServerContextFactory _serverFactory;
        private readonly ILogger<InitialStructureBuilder> _logger;
        private readonly InitialStructureBuilderOptions _options;

        public InitialStructureBuilder(IServerContextFactory serverFactory, IOptions<InitialStructureBuilderOptions> options,
            ILogger<InitialStructureBuilder> logger)
        {
            _serverFactory = serverFactory;
            _logger = logger;
            _options = options.Value;
        }

        public async Task BuildAsync()
        {
            var server = await _serverFactory.GetServerAsync();
            var coll = (await Content.LoadCollectionAsync("/Root/Content/SampleWorkspace/Memos", server)
                .ConfigureAwait(false)).ToArray();

            foreach (var content in coll)
            {
                _logger.LogTrace($"CONTENT: {content.Path}");
            }

            //TODO: import initial structure from _options.ImportFolderPath
        }
    }
}
