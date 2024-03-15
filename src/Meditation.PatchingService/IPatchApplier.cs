namespace Meditation.PatchingService
{
    public interface IPatchApplier
    {
        void ApplyPatch(int pid, PatchingConfiguration configuration);
    }
}
