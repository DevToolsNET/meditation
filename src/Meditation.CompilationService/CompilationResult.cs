using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Emit;

namespace Meditation.CompilationService
{
    public record CompilationResult(
        bool Success, 
        string OutputLog,
        ImmutableDictionary<string, byte[]> Assemblies,
        ImmutableDictionary<string, EmitResult> Result);
}
