using Meditation.PatchingService.Models;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using HarmonyLib;
using Meditation.PatchLibrary;
using Microsoft.Extensions.Logging;
using PatchInfo = Meditation.PatchingService.Models.PatchInfo;

namespace Meditation.PatchingService.Services
{
    internal class PatchListProvider : IPatchListProvider
    {
        private readonly IPatchStorage _patchStorage;
        private readonly AssemblyLoadContext _assemblyLoadContext;
        private ImmutableDictionary<AssemblyName, ImmutableArray<PatchInfo>> _patches;
        private readonly ILogger _logger;
        private bool _isReloadRequested;

        public PatchListProvider(IPatchStorage patchStorage, ILogger<PatchListProvider> logger)
        {
            _logger = logger;
            _patchStorage = patchStorage;
            _patches = ImmutableDictionary<AssemblyName, ImmutableArray<PatchInfo>>.Empty;
            _assemblyLoadContext = new AssemblyLoadContext(name: nameof(PatchListProvider));
            _isReloadRequested = true;
        }

        public void Reload()
        {
            _isReloadRequested = true;
        }

        public ImmutableDictionary<AssemblyName, ImmutableArray<PatchInfo>> GetAllPatches()
        {
            if (!_isReloadRequested)
                return _patches;
                    
            var files = Directory.GetFiles(_patchStorage.GetRootFolderForPatches())
                .Where(f => string.Equals(Path.GetExtension(f), ".dll", StringComparison.InvariantCultureIgnoreCase));

            var patchesBuilder = ImmutableDictionary.CreateBuilder<AssemblyName, ImmutableArray<PatchInfo>>();
            foreach (var file in files)
            {
                var patches = LoadPatches(file);
                if (patches.Length == 0)
                    continue;

                patchesBuilder.Add(patches[0].PatchName, patches);
            }

            _patches = patchesBuilder.ToImmutable();
            _isReloadRequested = false;
            return _patches;
        }

        private ImmutableArray<PatchInfo> LoadPatches(string fullPathName)
        {
            try
            {
                var assembly = _assemblyLoadContext.LoadFromAssemblyPath(fullPathName);
                var builder = ImmutableArray.CreateBuilder<PatchInfo>();
                foreach (var patch in assembly.GetTypes().Where(t => t.CustomAttributes.Any(a => a.AttributeType == typeof(HarmonyPatch))))
                {
                    var assemblyAttribute = patch.GetCustomAttribute<MeditationPatchAssemblyTargetAttribute>();
                    var typeAttribute = patch.GetCustomAttribute<MeditationPatchTypeTargetAttribute>();
                    var methodAttribute = patch.GetCustomAttribute<MeditationPatchMethodTargetAttribute>();
                    var methodParameterAttributes = patch.GetCustomAttributes<MeditationPatchMethodParameterTargetAttribute>();
                    var reversePatchAttribute = patch.GetCustomAttribute<HarmonyReversePatch>();
                    if (assemblyAttribute == null || typeAttribute == null || methodAttribute == null)
                    {
                        // Invalid patch metadata
                        _logger.LogWarning("Could not load patch type {patch} from assembly {assembly} due to missing/corrupted metadata.", patch.FullName, fullPathName);
                        continue;
                    }

                    builder.Add(new PatchInfo(
                        Path: fullPathName,
                        PatchName: assembly.GetName(),
                        TargetAssemblyName: assemblyAttribute.AssemblyFullName,
                        TargetFullyQualifiedTypeName: typeAttribute.TypeFullName,
                        IsReversible: reversePatchAttribute != null,
                        Method: new PatchedMethodInfo(
                            Name: methodAttribute.Name,
                            IsStatic: methodAttribute.IsStatic,
                            ParametersCount: methodAttribute.ParametersCount,
                            ParameterFullTypeNames: methodParameterAttributes
                                .OrderBy(p => p.Index)
                                .Select(p => p.TypeFullName)
                                .ToImmutableArray())));
                }

                return builder.ToImmutable();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load file {file} to search for patches.", fullPathName);
                return ImmutableArray<PatchInfo>.Empty;
            }
        }
    }
}
