using System.Collections.Generic;
using Meditation.MetadataLoaderService.Models;

namespace Meditation.MetadataLoaderService
{
    public interface IMetadataLoader
    {
        IEnumerable<AssemblyMetadataEntry> LoadMetadataFromProcess(IEnumerable<string> modulePath);
        AssemblyMetadataEntry LoadMetadataFromAssembly(string path);
        ModuleMetadataEntry GetCoreLibrary(MethodMetadataEntry method);
    }
}