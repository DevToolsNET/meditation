using Meditation.MetadataLoaderService.Models;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Meditation.CompilationService;

namespace Meditation.UI
{
    public interface IWorkspaceContext
    {
        event Action<MethodMetadataEntry>? WorkspaceCreated;
        event Action<MethodMetadataEntry>? WorkspaceDestroyed;

        MethodMetadataEntry? Method { get; }

        void CreateWorkspace(MethodMetadataEntry hookedMethod, string projectName, string assemblyName);
        void DestroyWorkspace();
        void AddDocument(string content, Encoding? encoding = null);
        Task<CompilationResult> Build(CancellationToken ct);
        byte[] GetProjectAssembly();
    }
}
