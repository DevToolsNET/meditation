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
        public event Action<ProcessId, MetadataEntryBase>? AssemblyOrNetModuleLoaded;

        public IProcessSnapshot? ProcessSnapshot { get; private set; }
        public ImmutableArray<AssemblyMetadataEntry> Assemblies { get; private set; }
        public ImmutableArray<ModuleMetadataEntry> NetModules { get; private set; }
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
            var assembliesBuilder = new List<AssemblyMetadataEntry>();
            var netModulesBuilder = new List<ModuleMetadataEntry>();
            var managedModulePaths = processSnapshot.EnumerateModules()
                .Where(m => m.IsManaged)
                .OrderBy(m => Path.GetFileName(m.FileName))
                .Select(m => m.FileName);

            foreach (var metadata in _metadataLoader.LoadMetadataFromProcess(managedModulePaths))
            {
                switch (metadata)
                {
                    case AssemblyMetadataEntry assemblyMetadata:
                        assembliesBuilder.Add(assemblyMetadata);
                        break;
                    case ModuleMetadataEntry moduleMetadata:
                        netModulesBuilder.Add(moduleMetadata);
                        break;
                    default:
                        throw new NotSupportedException(metadata.GetType().FullName);
                }

                AssemblyOrNetModuleLoaded?.Invoke(processSnapshot.ProcessId, metadata);
            }

            Assemblies = assembliesBuilder.ToImmutableArray();
            NetModules = netModulesBuilder.ToImmutableArray();
        }
    }
}
