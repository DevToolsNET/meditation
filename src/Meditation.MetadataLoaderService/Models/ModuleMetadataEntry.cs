using System.Collections.Immutable;

namespace Meditation.MetadataLoaderService.Models
{
    public record ModuleMetadataEntry(string Name, int Token, string Path, ImmutableArray<MetadataEntryBase> Children) : MetadataEntryBase(Name, Token, Children);
}
