using System.Collections.Immutable;

namespace Meditation.PatchingService.Models
{
    public record PatchedMethodInfo(string Name, bool IsStatic, int ParametersCount, ImmutableArray<string> ParameterFullTypeNames);
}
