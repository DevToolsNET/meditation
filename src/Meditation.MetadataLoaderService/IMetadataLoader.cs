using Meditation.MetadataLoaderService.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Meditation.MetadataLoaderService
{
    public interface IMetadataLoader
    {
        public ImmutableArray<string> ModulePaths { get; }
        IEnumerable<MetadataEntryBase> LoadMetadataFromProcess(IEnumerable<string> modulePaths);
        MetadataEntryBase LoadMetadataFromPath(string path);
        bool TryGetLoadedMetadataFromPath(string path, [NotNullWhen(returnValue: true)] out MetadataEntryBase? metadata);
    }
}