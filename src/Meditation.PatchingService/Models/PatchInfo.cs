using System.Reflection;

namespace Meditation.PatchingService.Models
{
    public record PatchInfo(string Path, AssemblyName PatchName, AssemblyName TargetAssemblyName, string TargetFullyQualifiedTypeName, bool IsReversible, PatchedMethodInfo Method);
}
