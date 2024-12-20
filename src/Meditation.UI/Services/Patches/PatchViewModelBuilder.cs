using Meditation.MetadataLoaderService;
using Meditation.MetadataLoaderService.Models;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Meditation.PatchingService.Models;
using PatchInfo = Meditation.PatchingService.Models.PatchInfo;

namespace Meditation.UI.Services.Patches
{
    internal class PatchViewModelBuilder : IPatchViewModelBuilder
    {
        private readonly IMetadataLoader _metadataLoader;

        public PatchViewModelBuilder(IMetadataLoader metadataLoader)
        {
            _metadataLoader = metadataLoader;
        }

        public PatchPackageMetadataEntry Build(AssemblyName patchAssembly, ImmutableArray<PatchInfo> patchInfos)
        {
            var childrenBuilder = ImmutableArray.CreateBuilder<MetadataEntryBase>();
            foreach (var patchesAssembly in patchInfos.GroupBy(p => p.TargetAssemblyName))
                childrenBuilder.Add(BuildAssemblyViewModel(patchesAssembly.Key, patchesAssembly.ToImmutableArray()));

            return new PatchPackageMetadataEntry(patchAssembly.Name!, patchAssembly.FullName, patchInfos.First().Path, childrenBuilder.ToImmutable());
        }

        private MetadataEntryBase BuildAssemblyViewModel(AssemblyName assemblyName, ImmutableArray<PatchInfo> patches)
        {
            AssemblyMetadataEntry assemblyMetadataEntry;

            try
            {
                var topLevelEntry = _metadataLoader.LoadMetadataFromAssemblyName(assemblyName);
                if (topLevelEntry is not AssemblyMetadataEntry assemblyEntry)
                    return new PatchAssemblyErrorMetadataEntry($"Unexpected element: {topLevelEntry.GetType()}.");

                assemblyMetadataEntry = assemblyEntry;
            }
            catch (Exception ex)
            {
                return new PatchAssemblyErrorMetadataEntry(ex.Message);
            }

            var childrenBuilder = ImmutableArray.CreateBuilder<MetadataEntryBase>();
            foreach (var patchesType in patches.GroupBy(p => p.TargetFullyQualifiedTypeName))
                childrenBuilder.Add(BuildTypeViewModel(assemblyMetadataEntry, patchesType.Key, patchesType.ToImmutableArray()));

            return new PatchAssemblyMetadataEntry(assemblyName.Name!, assemblyName.FullName, childrenBuilder.ToImmutable());
        }

        private MetadataEntryBase BuildTypeViewModel(AssemblyMetadataEntry assemblyMetadataEntry, string typeFullName, ImmutableArray<PatchInfo> patches)
        {
            var childrenBuilder = ImmutableArray.CreateBuilder<MetadataEntryBase>();

            if (assemblyMetadataEntry.Find(typeFullName) is not TypeMetadataEntry typeMetadataEntry)
                return new PatchTypeErrorMetadataEntry($"Could not find type \"{typeFullName}\".");

            foreach (var patch in patches)
                childrenBuilder.Add(BuildMethodViewModel(typeMetadataEntry, patch));

            return new PatchTypeMetadataEntry(typeMetadataEntry.Name, typeMetadataEntry.FullName, childrenBuilder.ToImmutable());
        }

        private static MetadataEntryBase BuildMethodViewModel(TypeMetadataEntry typeMetadataEntry, PatchInfo patch)
        {
            if (!TryFindTargetMethod(typeMetadataEntry, patch, out var targetMethod))
                return new PatchMethodErrorMetadataEntry($"Could not find method \"{patch.Method}\".");

            return new PatchMethodMetadataEntry(targetMethod.Name, targetMethod.FullName, targetMethod.Children);
        }

        private static bool TryFindTargetMethod(
            TypeMetadataEntry typeMetadataEntry, 
            PatchInfo patch, 
            [NotNullWhen(returnValue: true)] out MetadataEntryBase? targetMethod)
        {
            foreach (var member in typeMetadataEntry.Children)
            {
                if (member is not MethodMetadataEntry method)
                    continue;

                // Try match method metadata
                if (!method.Name.Equals(patch.Method.Name, StringComparison.Ordinal))
                    continue;
                if (method.ParametersCount != patch.Method.ParametersCount)
                    continue;
                if (method.IsStatic != patch.Method.IsStatic)
                    continue;
                var paramIndex = 0;
                if (method.EnumerateParameterTypeFullNames().Any(param => param != patch.Method.ParameterFullTypeNames[paramIndex++]))
                    continue;

                targetMethod = method;
                return true;
            }

            targetMethod = null;
            return false;
        }
    }
}
