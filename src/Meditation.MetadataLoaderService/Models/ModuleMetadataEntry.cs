using System.Collections.Immutable;

namespace Meditation.MetadataLoaderService.Models
{
    public record ModuleMetadataEntry(string Name, ImmutableArray<MetadataEntryBase> Children) : MetadataEntryBase(Name, Children);
}
