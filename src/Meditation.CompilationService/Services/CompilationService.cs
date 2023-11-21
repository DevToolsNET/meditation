using System.Collections.Generic;
using System.Text;
using Meditation.MetadataLoaderService.Models;

namespace Meditation.CompilationService.Services
{
    internal class CompilationService : ICompilationService
    {
        private enum PatchMethodType { Prefix, Postfix }

        public string GenerateCodeTemplateForPatch(MethodMetadataEntry method)
        {
            var sb = new StringBuilder();
            sb.AppendLine("class Patch");
            sb.AppendLine("{");
            {
                // Prefix
                sb.AppendLine($"\tpublic static void {method.Name}_Prefix({CreateArgsList(method, PatchMethodType.Prefix)})");
                sb.AppendLine("\t{");
                {
                    sb.AppendLine("\t\t// Implement prefix patch");
                }
                sb.AppendLine("\t}");
                sb.AppendLine();

                // Postfix
                sb.AppendLine($"\tpublic static void {method.Name}_Prefix({CreateArgsList(method, PatchMethodType.Postfix)})");
                sb.AppendLine("\t{");
                {
                    sb.AppendLine("\t\t// Implement prefix patch");
                }
                sb.AppendLine("\t}");
            }
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static string CreateArgsList(MethodMetadataEntry method, PatchMethodType type)
        {
            var arguments = new List<string>();
            if (!method.IsStatic)
                arguments.Add($"{method.DeclaringTypeFullName} __instance");
            if (method.HasParameters)
                arguments.Add("object[] __args");
            if (method.HasReturnType && type == PatchMethodType.Postfix)
                arguments.Add($"{method.ReturnType} __result");
            return string.Join(", ", arguments);
        }
    }
}
