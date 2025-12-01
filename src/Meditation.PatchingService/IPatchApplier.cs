using System.Threading.Tasks;

namespace Meditation.PatchingService
{
    public interface IPatchApplier
    {
        Task ApplyPatch(int pid, PatchingConfiguration configuration);
    }
}
