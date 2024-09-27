using HarmonyLib;
using Meditation.Bootstrap.Managed.Utils;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Meditation.Bootstrap.Managed
{
    public class EntryPoint
    {
        /// <summary>
        /// This is a managed entrypoint for hook. Once this gets executed, we are already in the target process default AppDomain / AssemblyLoadContext.
        /// You can use reflection to load new assemblies, inspect loaded types or setup high-level hooking support using, for example Harmony project.
        /// Note(invocations): this method will be executed dynamically by runtime on behalf of native module (Meditation.Bootstrap.Native) as a first step to initialize this hooking environment.
        /// Note(method signature): all managed hooks need to have a concrete signature. Static method, System.Int32 as return value, System.String as its only parameter.
        /// Note(return values): return 0 to indicate that the hook was successful. Use different values to indicate failures. These values will be interpreted by Meditation's injection service.
        /// References: for more details on these restrictions, see this: https://learn.microsoft.com/en-us/dotnet/framework/unmanaged-api/hosting/iclrruntimehost-executeindefaultappdomain-method
        /// </summary>
        /// <param name="input">Hook argument specified during injection</param>
        /// <returns>Zero on success, other values on failures</returns>
        public static int Hook(string input)
        {
            return HandleCommand(input, TryApplyPatchesFromAssembly);
        }

        /// <summary>
        /// This is a managed entrypoint for unhook. Once this gets executed, we are already in the target process default AppDomain / AssemblyLoadContext.
        /// You can use reflection to load new assemblies, inspect loaded types or setup high-level hooking support using, for example Harmony project.
        /// Note(invocations): this method will be executed dynamically by runtime on behalf of native module (Meditation.Bootstrap.Native) as a first step to initialize this hooking environment.
        /// Note(method signature): all managed hooks need to have a concrete signature. Static method, System.Int32 as return value, System.String as its only parameter.
        /// Note(return values): return 0 to indicate that the hook was successful. Use different values to indicate failures. These values will be interpreted by Meditation's injection service.
        /// References: for more details on these restrictions, see this: https://learn.microsoft.com/en-us/dotnet/framework/unmanaged-api/hosting/iclrruntimehost-executeindefaultappdomain-method
        /// </summary>
        /// <param name="input">Hook argument specified during injection</param>
        /// <returns>Zero on success, other values on failures</returns>
        public static int Unhook(string input)
        {
            return HandleCommand(input, TryReversePatchesFromAssembly);
        }

        private delegate bool Command(
            Harmony harmonyInstance, 
            Assembly patchAssembly, 
            Logger logger,
            [NotNullWhen(returnValue: false)] out ManagedHookErrorCode? errorCode);

        private static int HandleCommand(string input, Command command)
        {
            Logger? logger = null;

            try
            {
                if (!TryParseHookArguments(input, out var errorCode, out var arguments))
                    return (int)errorCode;

                logger = new Logger(arguments.LoggingFileName);
                logger.LogInfo($"Running in PID = {Process.GetCurrentProcess().Id}.");
                logger.LogInfo($"Arguments: {arguments}.");

                if (!TryLoadPatchAssembly(arguments.Argument, logger, out errorCode, out var patchAssembly))
                    return (int)errorCode;

                var harmonyInstance = new Harmony(MakeHarmonyIdentifier(arguments.UniqueIdentifier, patchAssembly));
                logger.LogInfo($"Patched methods count before execution: {Harmony.GetAllPatchedMethods().Count()}.");

                if (!command(harmonyInstance, patchAssembly, logger, out errorCode))
                    return (int)errorCode;

                logger.LogInfo($"Patched methods count after execution: {Harmony.GetAllPatchedMethods().Count()}.");
                logger.LogInfo($"Exiting from PID = {Process.GetCurrentProcess().Id}.");
                return (int)ManagedHookErrorCode.Ok;
            }
            catch (Exception ex)
            {
                return (int)HandleException(logger, ex);
            }
            finally
            {
                logger?.Dispose();
            }
        }

        private static bool TryParseHookArguments(
            string input, 
            [NotNullWhen(returnValue: false)] out ManagedHookErrorCode? errorCode,
            [NotNullWhen(returnValue: true)] out ManagedHookArguments? hookArgs)
        {
            if (!ManagedHookArguments.TryParse(input, out var error, out var arguments))
            {
                errorCode = error;
                hookArgs = null;
                return false;
            }

            errorCode = null;
            hookArgs = arguments;
            return true;
        }

        private static bool TryLoadPatchAssembly(
            string assemblyPath, 
            Logger logger,
            [NotNullWhen(returnValue: false)] out ManagedHookErrorCode? errorCode,
            [NotNullWhen(returnValue: true)] out Assembly? patchAssembly)
        {
            try
            {
                logger.LogInfo($"Loading assembly \"{assemblyPath}\".");
                patchAssembly = Assembly.LoadFile(assemblyPath);
                logger.LogInfo($"Successfully loaded assembly \"{assemblyPath}\".");
                errorCode = null;
                return true;
            }
            catch (Exception ex)
            {
                patchAssembly = null;
                errorCode = ManagedHookErrorCode.PatchAssemblyLoadException;
                logger.LogError($"An unhandled exception occurred while loading assembly \"{assemblyPath}\": {ex}.");
                return false;
            }
        }


        private static bool TryApplyPatchesFromAssembly(
            Harmony harmonyInstance, 
            Assembly patchAssembly, 
            Logger logger,
            [NotNullWhen(returnValue: false)] out ManagedHookErrorCode? errorCode)
        {
            try
            {
                logger.LogInfo($"Attempting to apply patches from \"{patchAssembly}\".");
                harmonyInstance.PatchAll(patchAssembly);
                logger.LogInfo("Successfully patched target program.");
                errorCode = null;
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError($"An unhandled exception occurred while patching using \"{patchAssembly}\": {ex}.");
                errorCode = ManagedHookErrorCode.UnhandledException_ApplyingPatches;
                return false;
            }
        }

        private static bool TryReversePatchesFromAssembly(
            Harmony harmonyInstance,
            Assembly patchAssembly,
            Logger logger,
            [NotNullWhen(returnValue: false)] out ManagedHookErrorCode? errorCode)
        {
            try
            {
                logger.LogInfo($"Attempting to reverse patches from \"{patchAssembly}\".");
                harmonyInstance.UnpatchAll(harmonyInstance.Id);
                logger.LogInfo("Successfully reverse-patched target program.");
                errorCode = null;
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError($"An unhandled exception occurred while reverse-patching using \"{patchAssembly}\": {ex}.");
                errorCode = ManagedHookErrorCode.UnhandledException_ReversingPatches;
                return false;
            }
        }

        private static ManagedHookErrorCode HandleException(Logger? logger, Exception exception)
        {
            logger?.LogError($"An unhandled exception occurred: {exception}.");
            return ManagedHookErrorCode.InternalError;
        }

        private static string MakeHarmonyIdentifier(string uniqueName, Assembly patchAssembly)
        {
            return $"{uniqueName}-{patchAssembly.FullName}";
        }
    }
}