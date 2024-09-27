using System.Collections.Immutable;
using Meditation.MetadataLoaderService.Models;

namespace Meditation.PatchingService.Models
{
    public sealed record PatchPackageMetadataEntry(string Name, string FullName, string Path, bool IsReversible, ImmutableArray<MetadataEntryBase> Children)
        : MetadataEntryBase(Name, FullName, Token: null, Children);
}
