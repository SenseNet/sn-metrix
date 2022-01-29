using SenseNet.IO.Implementations;

namespace SnMetrix.Client
{
    public class RepositoryOptions
    {
        public string Url { get; set; }
        public RepositoryAuthenticationOptions Authentication { get; set; } = new RepositoryAuthenticationOptions();
    }
}
