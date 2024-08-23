using System.Collections.Immutable;
using System.Reflection;
using Meditation.PatchingService.Models;

namespace Meditation.PatchingService
{
    public interface IPatchListProvider
    {
        ImmutableDictionary<AssemblyName, ImmutableArray<PatchInfo>> GetAllPatches();
        void Reload();
    }
}
