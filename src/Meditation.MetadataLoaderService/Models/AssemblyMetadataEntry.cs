using System;
using System.Collections.Immutable;
using System.Linq;

namespace Meditation.MetadataLoaderService.Models
{
    public record AssemblyMetadataEntry : MetadataEntryBase
    {
        public Version Version { get; }
        public ModuleMetadataEntry ManifestModule { get; }

        public AssemblyMetadataEntry(string name, string fullName, Version version, ImmutableArray<MetadataEntryBase> children)
            : base(name, fullName, Token: null, children)
        {
            Version = version;
            FullName = fullName;
            if (children.FirstOrDefault() is not ModuleMetadataEntry manifestModule)
                throw new ArgumentException($"Assembly \"{FullName}\" is corrupted.");
            ManifestModule = manifestModule;
        }
    }
}
