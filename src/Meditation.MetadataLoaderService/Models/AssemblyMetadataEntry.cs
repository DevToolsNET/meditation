using System;
using System.Collections.Immutable;

namespace Meditation.MetadataLoaderService.Models
{
    public record AssemblyMetadataEntry(string Name, Version Version, AssemblyToken AssemblyToken, string FullName, ImmutableArray<MetadataEntryBase> Children)
        : MetadataEntryBase(Name, new MetadataToken(AssemblyToken.Value), Children);
}
