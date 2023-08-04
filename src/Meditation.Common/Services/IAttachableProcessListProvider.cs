using System.Collections.Immutable;
using Meditation.Common.Models;

namespace Meditation.Common.Services
{
    public interface IAttachableProcessListProvider
    {
        ImmutableArray<ProcessInfo> GetAllAttachableProcesses();
    }
}
