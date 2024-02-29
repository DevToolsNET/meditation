using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Meditation.Bootstrap.Managed
{
    public class EntryPoint
    {
        /// <summary>
        /// This is a managed entrypoint for a "hello-world style" hook. Once this gets executed, we are already in the target process default AppDomain / AssemblyLoadContext.
        /// You can use reflection to load new assemblies, inspect loaded types or setup high-level hooking support using, for example Harmony project.
        /// Note(invocations): this method will be executed dynamically by runtime on behalf of native module (Meditation.Bootstrap.Native) as a first step to initialize this hooking environment.
        /// Note(method signature): all managed hooks need to have a concrete signature. Static method, System.Int32 as return value, System.String as its only parameter.
        /// Note(return values): return 0 to indicate that the hook was successful. Use different values to indicate failures. These values will be interpreted by Meditation's injection service.
        /// References: for more details on these restrictions, see this: https://learn.microsoft.com/en-us/dotnet/framework/unmanaged-api/hosting/iclrruntimehost-executeindefaultappdomain-method
        /// </summary>
        /// <param name="argument">Hook argument specified during injection</param>
        /// <returns>Zero on success, other values on failures</returns>
        public static int Hook(string argument)
        {
            try
            {
                var harmony = new Harmony("Test");
                Harmony.DEBUG = true;
                FileLog.Log($"Called with args: {argument}");
                harmony.PatchAll(Assembly.LoadFile(argument));
                FileLog.Log($"Patched methods: {string.Join(",", Harmony.GetAllPatchedMethods().Select(m => m.FullDescription()))}");
                return 0;
            }
            finally
            {
                FileLog.FlushBuffer();;
            }
        }
    }
}