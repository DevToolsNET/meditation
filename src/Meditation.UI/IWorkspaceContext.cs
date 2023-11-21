using Meditation.MetadataLoaderService.Models;
using System;

namespace Meditation.UI
{
    public interface IWorkspaceContext
    {
        event Action<MethodMetadataEntry>? WorkspaceCreated;
        event Action<MethodMetadataEntry>? WorkspaceDestroyed;

        MethodMetadataEntry? Method { get; }

        void CreateWorkspace(MethodMetadataEntry method);
        void DestroyWorkspace();
    }
}
