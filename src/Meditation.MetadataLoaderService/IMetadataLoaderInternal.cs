using System.Collections.Generic;
using dnlib.DotNet;

namespace Meditation.MetadataLoaderService
{
    internal interface IMetadataLoaderInternal : IMetadataLoader
    {
        AssemblyResolver AssemblyResolver { get; }
        ICollection<ModuleDef> LoadedModules { get; }
    }
}
