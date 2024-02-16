using System.Collections.Immutable;
using Meditation.PatchingService.Models;

namespace Meditation.PatchingService
{
    public interface IPatchListProvider
    {
        public ImmutableArray<PatchInfo> GetAllPatches();
        public void Reload();
    }
}
