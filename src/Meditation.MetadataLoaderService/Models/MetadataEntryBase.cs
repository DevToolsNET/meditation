using System.Collections.Immutable;

namespace Meditation.MetadataLoaderService.Models
{
    public abstract record MetadataEntryBase(
        string Name,
        int Token,
        ImmutableArray<MetadataEntryBase> Children);
}
