using System.Collections.Immutable;

namespace Meditation.MetadataLoaderService.Models
{
    public record AssemblyMetadataEntry(string Name, int Token, string FullName, ImmutableArray<MetadataEntryBase> Children) : MetadataEntryBase(Name, Token, Children);
}
