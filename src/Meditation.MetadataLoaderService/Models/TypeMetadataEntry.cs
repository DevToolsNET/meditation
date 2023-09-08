using System.Collections.Immutable;

namespace Meditation.MetadataLoaderService.Models
{
    public record TypeMetadataEntry(string Name, string FullName, int Token, ImmutableArray<MetadataEntryBase> Children) : MetadataEntryBase(Name, Token, Children);
}
