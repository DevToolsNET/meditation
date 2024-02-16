using Meditation.PatchingService.Models;

namespace Meditation.PatchingService
{
    public interface IPatchApplier
    {
        void ApplyPatch(int pid, PatchInfo patch);
    }
}
