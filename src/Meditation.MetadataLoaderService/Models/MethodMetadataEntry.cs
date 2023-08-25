using System.Collections.Immutable;

namespace Meditation.MetadataLoaderService.Models
{
    public record MethodMetadataEntry(string Name, ImmutableArray<MetadataEntryBase> Children) : MetadataEntryBase(Name, Children);
}
