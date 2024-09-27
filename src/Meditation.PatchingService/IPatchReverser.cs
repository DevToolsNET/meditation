namespace Meditation.PatchingService
{
    public interface IPatchReverser
    {
        void ReversePatch(int pid, PatchingConfiguration configuration);
    }
}
