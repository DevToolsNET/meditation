using Meditation.MetadataLoaderService.Models;

namespace Meditation.MetadataLoaderService
{
    public interface IMetadataLoader
    {
        AssemblyMetadataEntry LoadMetadataFromAssembly(string path);
    }
}