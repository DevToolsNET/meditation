using System;
using Meditation.CompilationService.Utils;
using Meditation.MetadataLoaderService.Models;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Meditation.PatchLibrary;

namespace Meditation.CompilationService.Services
{
    internal class CodeTemplateProvider : ICodeTemplateProvider
    {
        private enum PatchMethodType { Prefix, Postfix }

        public string GenerateCodeTemplateForPatch(MethodMetadataEntry method)
        {
            var sb = new FormattedStringBuilder();
            sb.AppendLine($"using {typeof(Harmony).Namespace};");
            sb.AppendLine($"using {typeof(MeditationPatchAssemblyTargetAttribute).Namespace};");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Reflection;");
            sb.AppendLine();
            sb.AppendLine($"[assembly: {nameof(MeditationPatchAssemblyTargetAttribute)}(\"{method.AssemblyName}\")]");
            sb.AppendLine($"[assembly: {nameof(MeditationPatchTypeTargetAttribute)}(\"{method.DeclaringTypeFullName}\")]");
            sb.AppendLine($"[assembly: {nameof(MeditationPatchMethodTargetAttribute)}(" +
                          $"name: \"{method.Name}\", " +
                          $"isStatic: {method.IsStatic.ToString().ToLowerInvariant()}, " +
                          $"parametersCount: {method.ParametersCount})]");
            var parameterIndex = 0;
            foreach (var parameter in method.ParameterTypeFullNames)
                sb.AppendLine($"[assembly: {nameof(MeditationPatchMethodParameterTargetAttribute)}(index: {parameterIndex++}, typeFullName: \"{parameter}\")]");
            sb.AppendLine();
            sb.AppendLine($"[{nameof(HarmonyPatch)}]");
            sb.AppendLine("public sealed class Patch");
            using (new FormattedStringBuilderIndentationScope(sb))
            {
                AddTargetMethod(sb, method);
                sb.AppendLine();

                AddPrepareMethod(sb);
                sb.AppendLine();

                AddCleanupMethod(sb);
                sb.AppendLine();

                AddPrefixMethod(sb, method);
                sb.AppendLine();

                AddPostfixMethod(sb, method);
            }

            return sb.ToString();
        }

        private static void AddTargetMethod(FormattedStringBuilder sb, MethodMetadataEntry method)
        {
            sb.AppendLine($"[{nameof(HarmonyTargetMethod)}]");
            sb.AppendLine("public static MethodBase GetTargetMethod()");
            using (new FormattedStringBuilderIndentationScope(sb))
            {
                var methodTypes = string.Join(',', method.ParameterTypeFullNames.Select(t => $"typeof({t})"));
                var bindingFlags = (method.IsStatic) ? $"{nameof(BindingFlags)}.{nameof(BindingFlags.Static)}" : $"{nameof(BindingFlags)}.{nameof(BindingFlags.Instance)}";
                bindingFlags += $" | {nameof(BindingFlags)}.{nameof(BindingFlags.Public)} | {nameof(BindingFlags)}.{nameof(BindingFlags.NonPublic)}";

                sb.AppendLine($"return typeof({method.DeclaringTypeFullName}).GetMethod(");
                sb.AppendLine($"\tname: \"{method.Name}\",");
                sb.AppendLine($"\tbindingAttr: {bindingFlags},");
                sb.AppendLine($"\tbinder: {nameof(Type)}.{nameof(Type.DefaultBinder)},");
                sb.AppendLine($"\ttypes: {((methodTypes.Length > 0) ? $"new[] {{ {methodTypes} }}" : "Array.Empty<Type>()")},");
                sb.AppendLine("\tmodifiers: null);");
            }
        }

        private static void AddPrepareMethod(FormattedStringBuilder sb)
        {
            sb.AppendLine($"[{nameof(HarmonyPrepare)}]");
            sb.AppendLine("public static bool TryInitialize()");
            using (new FormattedStringBuilderIndentationScope(sb))
            {
                sb.AppendLine("// Perform patch initialization here");
                sb.AppendLine("return true;");
            }
        }

        private static void AddCleanupMethod(FormattedStringBuilder sb)
        {
            sb.AppendLine($"[{nameof(HarmonyCleanup)}]");
            sb.AppendLine("public static void Cleanup()");
            using (new FormattedStringBuilderIndentationScope(sb))
            {
                sb.AppendLine("// Perform patch cleanup here");
            }
        }

        private static void AddPrefixMethod(FormattedStringBuilder sb, MethodMetadataEntry method)
        {
            sb.AppendLine("[HarmonyPrefix]");
            sb.AppendLine($"public static void MethodPrefix({CreateArgsList(method, PatchMethodType.Prefix)})");
            using (new FormattedStringBuilderIndentationScope(sb))
            {
                sb.AppendLine("// TODO: implement prefix patch");
            }
        }

        private static void AddPostfixMethod(FormattedStringBuilder sb, MethodMetadataEntry method)
        {
            sb.AppendLine("[HarmonyPostfix]");
            sb.AppendLine($"public static void MethodPostfix({CreateArgsList(method, PatchMethodType.Postfix)})");
            using (new FormattedStringBuilderIndentationScope(sb))
            {
                sb.AppendLine("// TODO: implement postfix patch");
            }
        }

        private static string CreateArgsList(MethodMetadataEntry method, PatchMethodType type)
        {
            var arguments = new List<string>();
            if (!method.IsStatic)
                arguments.Add("object __instance");
            if (method.HasParameters)
                arguments.Add("object[] __args");
            if (method.HasReturnType && type == PatchMethodType.Postfix)
                arguments.Add("object __result");
            return string.Join(", ", arguments);
        }
    }
}
