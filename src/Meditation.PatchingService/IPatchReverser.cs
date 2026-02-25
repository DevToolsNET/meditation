using System.Threading.Tasks;

namespace Meditation.PatchingService
{
    public interface IPatchReverser
    {
        Task ReversePatch(int pid, PatchingConfiguration configuration);
    }
}
