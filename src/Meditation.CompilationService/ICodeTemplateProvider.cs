using Meditation.MetadataLoaderService.Models;

namespace Meditation.CompilationService
{
    public interface ICodeTemplateProvider
    {
        string GenerateCodeTemplateForPatch(MethodMetadataEntry method);
    }
}
