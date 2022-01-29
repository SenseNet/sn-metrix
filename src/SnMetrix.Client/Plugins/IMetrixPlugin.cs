using System.Threading.Tasks;

namespace SnMetrix.Client.Plugins
{
    public interface IMetrixPlugin
    {
        Task ExecuteAsync();
    }
}
