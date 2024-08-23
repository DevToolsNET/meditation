using Meditation.MetadataLoaderService.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Meditation.MetadataLoaderService
{
    public interface IMetadataLoader
    {
        public ImmutableArray<string> ModulePaths { get; }
        IEnumerable<MetadataEntryBase> LoadMetadataFromProcess(IEnumerable<string> modulePaths);
        MetadataEntryBase LoadMetadataFromPath(string path);
        MetadataEntryBase LoadMetadataFromAssemblyName(AssemblyName assemblyName);
        bool TryGetLoadedMetadataFromPath(string path, [NotNullWhen(returnValue: true)] out MetadataEntryBase? metadata);
    }
}