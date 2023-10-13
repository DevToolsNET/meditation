using System.Collections.Immutable;

namespace Meditation.MetadataLoaderService.Models
{
    public abstract record MetadataEntryBase(
        string Name,
        MetadataToken Token,
        ImmutableArray<MetadataEntryBase> Children);
}
