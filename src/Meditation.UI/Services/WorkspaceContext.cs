using Meditation.CompilationService;
using Meditation.MetadataLoaderService;
using Meditation.MetadataLoaderService.Models;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.UI.Services
{
    internal class WorkspaceContext : IWorkspaceContext, IDisposable
    {
        public event Action<MethodMetadataEntry>? WorkspaceCreated;
        public event Action<MethodMetadataEntry>? WorkspaceDestroyed;
        private readonly IServiceProvider _serviceProvider;
        private ICompilationService? _compilationService;
        private IMetadataLoader? _metadataLoader;
        private ProjectId? _mainProjectId;
        private bool _disposed;

        public WorkspaceContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public MethodMetadataEntry? Method { get; private set; }

        public void CreateWorkspace(MethodMetadataEntry hookedMethod, string projectName, string assemblyName)
        {
            EnsureNotDisposed();

            Method = hookedMethod;
            _compilationService = _serviceProvider.GetRequiredService<ICompilationService>();
            _metadataLoader = _serviceProvider.GetRequiredService<IMetadataLoader>();

            // Prepare metadata references for the workspace
            // TODO: we should ideally work with so called "reference assemblies" and not directly implementations, however it works
            var coreLibPath = _metadataLoader.GetCoreLibrary(hookedMethod).Path;
            var targetPath = hookedMethod.ModulePath;

            _mainProjectId = _compilationService!.AddProject(projectName, assemblyName, coreLibPath, ImmutableArray.Create(targetPath));
            WorkspaceCreated?.Invoke(hookedMethod);
        }

        public void DestroyWorkspace()
        {
            EnsureNotDisposed();
            EnsureWorkspaceCreated();

            WorkspaceDestroyed?.Invoke(Method!);
            _compilationService = null;
            _metadataLoader = null;
            _mainProjectId = null;
            Method = null;
        }

        public void AddDocument(string content, Encoding? encoding = null)
        {
            EnsureNotDisposed();
            EnsureWorkspaceCreated();

            _compilationService!.AddDocument(_mainProjectId!, content, encoding ?? Encoding.UTF8);
        }

        public Task<CompilationResult> Build(CancellationToken ct)
        {
            EnsureNotDisposed();
            EnsureWorkspaceCreated();

            return _compilationService!.Build(ct);
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(WorkspaceContext));
        }

        private void EnsureWorkspaceCreated()
        {
            if (_compilationService == null || _mainProjectId == null || Method == null)
                throw new InvalidOperationException("Workspace must be created before calling build.");
        }

        public byte[] GetProjectAssembly()
        {
            EnsureNotDisposed();
            EnsureWorkspaceCreated();

            return _compilationService!.GetProjectAssembly(_mainProjectId!);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            if (_compilationService != null)
                DestroyWorkspace();
            _disposed = true;
        }
    }
}
