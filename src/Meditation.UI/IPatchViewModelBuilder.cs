using System.Collections.Immutable;
using System.Reflection;
using Meditation.PatchingService.Models;

namespace Meditation.UI
{
    public interface IPatchViewModelBuilder
    {
        PatchPackageMetadataEntry Build(AssemblyName patchAssembly, ImmutableArray<PatchInfo> patchInfos);
    }
}
