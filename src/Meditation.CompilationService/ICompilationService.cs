using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.CompilationService
{
    public interface ICompilationService
    {
        ProjectId AddProject(string projectName, string assemblyName, string coreLibraryPath, ImmutableArray<string> additionalAssemblyReferences);
        DocumentId AddDocument(ProjectId projectId, string content, Encoding? encoding = null);
        Task<CompilationResult> Build(CancellationToken ct);
        byte[] GetProjectAssembly(ProjectId projectId);
    }
}