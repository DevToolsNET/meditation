using Meditation.AttachProcessService;
using Meditation.MetadataLoaderService;
using Meditation.MetadataLoaderService.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Meditation.AttachProcessService.Models;

namespace Meditation.UI.Services
{
    internal class AttachedProcessContext : IAttachedProcessContext
    {
        public event Action<ProcessId>? ProcessAttaching;
        public event Action<ProcessId>? ProcessAttached;
        public event Action<ProcessId>? ProcessDetached;
        public event Action<ProcessId, AssemblyMetadataEntry>? AssemblyLoaded;

        public IProcessSnapshot? ProcessSnapshot { get; private set; }
        public ImmutableArray<AssemblyMetadataEntry> Assemblies { get; private set; }
        private readonly IMetadataLoader _metadataLoader;
        private readonly object _syncObject;

        public AttachedProcessContext(IMetadataLoader metadataLoader)
        {
            _metadataLoader = metadataLoader;
            _syncObject = new object();
        }

        public void Initialize(IProcessSnapshot processSnapshot)
        {
            lock (_syncObject)
            {
                if (ProcessSnapshot is { } attachedProcess)
                    throw new InvalidOperationException($"An existing process with PID={attachedProcess.ProcessId} is already attached.");

                ProcessAttaching?.Invoke(processSnapshot.ProcessId);
                LoadAssemblies(processSnapshot);
                ProcessSnapshot = processSnapshot;
                ProcessAttached?.Invoke(processSnapshot.ProcessId);
            }
        }

        public void Reset()
        {
            lock (_syncObject)
            {
                if (ProcessSnapshot is not { } attachedProcess)
                    throw new InvalidOperationException("No process is currently attached.");

                ProcessSnapshot.Dispose();
                ProcessSnapshot = null;
                Assemblies = ImmutableArray<AssemblyMetadataEntry>.Empty;
                ProcessDetached?.Invoke(attachedProcess.ProcessId);
            }
        }

        private void LoadAssemblies(IProcessSnapshot processSnapshot)
        {
            var builder = new List<AssemblyMetadataEntry>();
            var managedModulePaths = processSnapshot.GetModules()
                .Where(m => m.IsManaged)
                .OrderBy(m => Path.GetFileName(m.FileName))
                .Select(m => m.FileName);

            foreach (var assemblyMetadata in _metadataLoader.LoadMetadataFromProcess(managedModulePaths))
            {
                AssemblyLoaded?.Invoke(processSnapshot.ProcessId, assemblyMetadata);
                builder.Add(assemblyMetadata);
            }

            Assemblies = builder.ToImmutableArray();
        }
    }
}
