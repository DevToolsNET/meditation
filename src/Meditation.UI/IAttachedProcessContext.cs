using System;
using System.Collections.Immutable;
using Meditation.AttachProcessService;
using Meditation.AttachProcessService.Models;
using Meditation.MetadataLoaderService.Models;

namespace Meditation.UI
{
    public interface IAttachedProcessContext
    {
        event Action<ProcessId>? ProcessAttaching;
        event Action<ProcessId>? ProcessAttached;
        event Action<ProcessId>? ProcessDetached;
        event Action<ProcessId, AssemblyMetadataEntry>? AssemblyLoaded;

        IProcessSnapshot? ProcessSnapshot { get; }
        ImmutableArray<AssemblyMetadataEntry> Assemblies { get; }

        void Initialize(IProcessSnapshot processSnapshot);
        void Reset();
    }
}
