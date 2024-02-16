using Meditation.PatchingService.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using Meditation.PatchLibrary;

namespace Meditation.PatchingService.Services
{
    internal class PatchListProvider : IPatchListProvider
    {
        private readonly IPatchStorage _patchStorage;
        private readonly AssemblyLoadContext _assemblyLoadContext;
        private ImmutableArray<PatchInfo> _patches;
        private bool _isReloadRequested;

        public PatchListProvider(IPatchStorage patchStorage)
        {
            _isReloadRequested = true;
            _patchStorage = patchStorage;
            _assemblyLoadContext = new AssemblyLoadContext(name: nameof(PatchListProvider));
        }

        public void Reload()
        {
            _isReloadRequested = true;
        }

        public ImmutableArray<PatchInfo> GetAllPatches()
        {
            if (!_isReloadRequested)
                return _patches;
                    
            var files = Directory.GetFiles(_patchStorage.GetRootFolderForPatches())
                .Where(f => string.Equals(Path.GetExtension(f), ".dll", StringComparison.InvariantCultureIgnoreCase));

            var patchesBuilder = new List<PatchInfo>();
            foreach (var file in files)
            {
                if (!TryLoadPatch(file, out var patchInfo))
                    continue;
                patchesBuilder.Add(patchInfo);
            }

            _patches = patchesBuilder.ToImmutableArray();
            _isReloadRequested = false;
            return _patches;
        }

        private bool TryLoadPatch(string fullPathName, [NotNullWhen(returnValue: true)] out PatchInfo? patchInfo)
        {
            try
            {
                var assembly = _assemblyLoadContext.LoadFromAssemblyPath(fullPathName);
                var attributes = assembly.GetCustomAttributes(inherit: false);
                var targetAssemblyAttribute = attributes.SingleOrDefault(a => a is MeditationPatchAssemblyTargetAttribute) as MeditationPatchAssemblyTargetAttribute;
                var targetTypeAttribute = attributes.SingleOrDefault(a => a is MeditationPatchTypeTargetAttribute) as MeditationPatchTypeTargetAttribute;
                var targetMethodAttribute = attributes.SingleOrDefault(a => a is MeditationPatchMethodTargetAttribute) as MeditationPatchMethodTargetAttribute;
                if (targetAssemblyAttribute == null || targetTypeAttribute == null || targetMethodAttribute == null)
                {
                    // Not a patch assembly
                    patchInfo = null;
                    return false;
                }

                // Discovered a patch
                patchInfo = new PatchInfo(
                    Path: fullPathName,
                    TargetFullAssemblyName: targetAssemblyAttribute.AssemblyFullName,
                    Method: new PatchedMethodInfo(
                        Name: targetMethodAttribute.Name,
                        IsStatic: targetMethodAttribute.IsStatic,
                        ParametersCount: targetMethodAttribute.ParametersCount,
                        ParameterFullTypeNames: default));

                return true;
            }
            catch
            {
                // Could not load dll
                // FIXME [#16]: logging
                patchInfo = null;
                return false;
            }
        }
    }
}
