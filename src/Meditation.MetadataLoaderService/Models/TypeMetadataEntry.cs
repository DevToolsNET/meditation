using System.Collections.Immutable;

namespace Meditation.MetadataLoaderService.Models
{
    public record TypeMetadataEntry(string Name, string FullName, ImmutableArray<MetadataEntryBase> Children) : MetadataEntryBase(Name, Children);
}
