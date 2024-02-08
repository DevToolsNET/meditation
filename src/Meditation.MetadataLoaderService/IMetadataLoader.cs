using System.Collections.Generic;
using Meditation.MetadataLoaderService.Models;

namespace Meditation.MetadataLoaderService
{
    public interface IMetadataLoader
    {
        IEnumerable<MetadataEntryBase> LoadMetadataFromProcess(IEnumerable<string> modulePaths);
        MetadataEntryBase LoadMetadataFromPath(string path);
        ModuleMetadataEntry GetCoreLibrary(MethodMetadataEntry method);
    }
}