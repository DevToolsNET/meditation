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
        public event Action<MethodMetadataEntry>? WorkspaceCreating;
        public event Action<MethodMetadataEntry>? WorkspaceCreated;
        public event Action<MethodMetadataEntry>? WorkspaceDestroyed;
        private readonly IServiceProvider _serviceProvider;
        private ICompilationService? _compilationService;
        private IDependencyResolver? _dependencyResolver;
        private IMetadataLoader? _metadataLoader;
        private ProjectId? _mainProjectId;
        private bool _disposed;

        public WorkspaceContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public MethodMetadataEntry? Method { get; private set; }

        public async Task CreateWorkspace(MethodMetadataEntry hookedMethod, string projectName, string assemblyName, CancellationToken ct)
        {
            EnsureNotDisposed();

            WorkspaceCreating?.Invoke(hookedMethod);
            Method = hookedMethod;
            _compilationService = _serviceProvider.GetRequiredService<ICompilationService>();
            _metadataLoader = _serviceProvider.GetRequiredService<IMetadataLoader>();
            _dependencyResolver = _serviceProvider.GetRequiredService<IDependencyResolver>();

            // Prepare metadata references for the workspace
            var targetPath = hookedMethod.ModulePath;

            if (!_metadataLoader.TryGetLoadedMetadataFromPath(targetPath, out var targetMetadataEntry))
                throw new Exception($"Can not resolve metadata for module \"{targetPath}\".");
            var targetModule = (targetMetadataEntry is AssemblyMetadataEntry ame) ? ame.ManifestModule :
                               targetMetadataEntry as ModuleMetadataEntry ??
                               throw new NotSupportedException($"Unsupported metadata entry \"{targetMetadataEntry.GetType()}\".");

            var referencesBuilder = ImmutableArray.CreateBuilder<string>();
            var meditationDependencies = _dependencyResolver.MeditationAssemblies;
            var targetDependencies = await Task.Run(() => _dependencyResolver.GetReferencedAssemblies(targetModule), ct);
            referencesBuilder.Add(targetPath);
            referencesBuilder.AddRange(meditationDependencies);
            referencesBuilder.AddRange(targetDependencies);

            _mainProjectId = _compilationService!.AddProject(projectName, assemblyName, referencesBuilder.ToImmutable());
            WorkspaceCreated?.Invoke(hookedMethod);
        }

        public void DestroyWorkspace()
        {
            EnsureNotDisposed();
            EnsureWorkspaceCreated();

            WorkspaceDestroyed?.Invoke(Method!);
            _compilationService = null;
            _dependencyResolver = null;
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
