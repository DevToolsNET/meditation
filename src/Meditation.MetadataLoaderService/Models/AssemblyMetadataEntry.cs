using dnlib.DotNet;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Meditation.MetadataLoaderService.Models
{
    public record AssemblyMetadataEntry : MetadataEntryBase
    {
        public Version Version => AssemblyDef.Version;
        public ModuleMetadataEntry ManifestModule { get; }
        internal readonly AssemblyDef AssemblyDef;

        public AssemblyMetadataEntry(AssemblyDef assemblyDef, ImmutableArray<MetadataEntryBase> children)
            : base(assemblyDef.Name, new MetadataToken(assemblyDef.MDToken.ToInt32()), children)
        {
            AssemblyDef = assemblyDef;
            if (children.FirstOrDefault() is not ModuleMetadataEntry manifestModule)
                throw new ArgumentException($"Assembly \"{assemblyDef.FullName}\" is corrupted.");
            ManifestModule = manifestModule;
        }
    }
}
