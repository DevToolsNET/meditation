using Meditation.MetadataLoaderService.Models;

namespace Meditation.CompilationService
{
    public interface ICompilationService
    {
        public string GenerateCodeTemplateForPatch(MethodMetadataEntry method);
    }
}