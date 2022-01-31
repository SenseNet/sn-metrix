using System;
using System.Threading.Tasks;

namespace SnMetrix.Client.Plugins
{
    public interface IMetrixPlugin
    {
        int OperationCount { get; }
        int MaxDegreeOfParallelism { get; }

        Task ExecuteAsync(IProgress<(int, TimeSpan)> progress);
    }
}
