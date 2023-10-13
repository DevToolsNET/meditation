using System.Collections.Immutable;

namespace Meditation.MetadataLoaderService.Models
{
    public record ModuleMetadataEntry(string Name, ModuleToken ModuleToken, string Path, ImmutableArray<MetadataEntryBase> Children)
        : MetadataEntryBase(Name, new MetadataToken(ModuleToken.Value), Children);
}
