using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SenseNet.Client;
using SenseNet.Client.Authentication;

namespace SnMetrix.Client
{
    public interface IServerContextFactory
    {
        Task<ServerContext> GetServerAsync();
    }

    //TODO: Move server context factory to the official client library.

    public class ServerContextFactory : IServerContextFactory
    {
        private readonly ITokenStore _tokenStore;
        private readonly RepositoryOptions _options;
        private ServerContext _server;
        private readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);

        public ServerContextFactory(ITokenStore tokenStore, IOptions<RepositoryOptions> options)
        {
            _tokenStore = tokenStore;
            _options = options.Value;
        }

        [SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
        public async Task<ServerContext> GetServerAsync()
        {
            if (_server != null)
                return _server;

            await _asyncLock.WaitAsync();

            try
            {
                if (_server != null)
                    return _server;

                _server = await GetAuthenticatedServerAsync().ConfigureAwait(false);
            }
            finally
            {
                _asyncLock.Release();
            }

            return _server;
        }

        private async Task<ServerContext> GetAuthenticatedServerAsync()
        {
            var server = new ServerContext
            {
                Url = _options.Url.AppendSchema()
            };

            server.Authentication.AccessToken = await _tokenStore.GetTokenAsync(server,
                _options.Authentication.ClientId, _options.Authentication.ClientSecret);

            return server;
        }
    }
}
