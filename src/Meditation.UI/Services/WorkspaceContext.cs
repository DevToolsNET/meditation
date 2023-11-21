using Meditation.MetadataLoaderService.Models;
using System;

namespace Meditation.UI.Services
{
    internal class WorkspaceContext : IWorkspaceContext
    {
        public event Action<MethodMetadataEntry>? WorkspaceCreated;
        public event Action<MethodMetadataEntry>? WorkspaceDestroyed;

        public MethodMetadataEntry? Method { get; private set; }

        public void CreateWorkspace(MethodMetadataEntry method)
        {
            Method = method;
            WorkspaceCreated?.Invoke(method);
        }

        public void DestroyWorkspace()
        {
            WorkspaceDestroyed?.Invoke(Method);
            Method = null;
        }
    }
}
