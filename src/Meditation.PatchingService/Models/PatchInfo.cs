namespace Meditation.PatchingService.Models
{
    public record PatchInfo(string Path, string TargetFullAssemblyName, PatchedMethodInfo Method);
}
