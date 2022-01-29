using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SenseNet.Client;

namespace SnMetrix.Client.Plugins
{
    public class FlatContentImporter : IMetrixPlugin
    {
        private readonly IServerContextFactory _serverFactory;
        private readonly ILogger<FlatContentImporter> _logger;

        public FlatContentImporter(IServerContextFactory serverFactory, ILogger<FlatContentImporter> logger)
        {
            _serverFactory = serverFactory;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            var server = await _serverFactory.GetServerAsync();

            // perform the real batch import and measurement here
            for (var i = 0; i < 10; i++)
            {
                var content = Content.CreateNew("/Root/Content/MetrixWorkspace/flatcontent", 
                    "FlatContent", Guid.NewGuid().ToString(), null, server);

                await content.SaveAsync();

                _logger.LogTrace($"Content {content.Path} saved.");
            }
        }
    }
}
