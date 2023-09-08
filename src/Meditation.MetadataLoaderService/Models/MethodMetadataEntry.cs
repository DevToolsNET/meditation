using System.Collections.Immutable;

namespace Meditation.MetadataLoaderService.Models
{
    public record MethodMetadataEntry(string Name, int Token, ImmutableArray<MetadataEntryBase> Children) : MetadataEntryBase(Name, Token, Children);
}
