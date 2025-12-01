using CliWrap;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Meditation.Interop;
using System.IO;
using System.Linq;

namespace Meditation.InjectorService.Services.Linux
{
    internal class LinuxProcessInjector : IProcessInjector
    {
        private const int DLOPEN_RTLD_NOW = 0x2;
        private const int DLOPEN_RTLD_GLOBAL = 0x100;
        private const string ShellExecutable = "/bin/sh";
        private const string GdbTestCommand = "gdb --version";
        private const string GdbInjectModuleCommandTemplate = """echo 'print dlopen("{0}", {1})' | gdb -p {2}""";

        public async Task<SafeHandle> TryInjectModule(int pid, string modulePath)
        {
            if (!await IsGdbInstalled())
            {
                // Could not find usable gdb
                // FIXME [#16]: logging
                return GenericSafeHandle.Invalid;
            }

            try
            {
                // Inject module into remote process
                var flags = DLOPEN_RTLD_NOW | DLOPEN_RTLD_GLOBAL;
                var gdbInjectCommand = string.Format(GdbInjectModuleCommandTemplate, modulePath, flags, pid);
                await Cli.Wrap(ShellExecutable)
                    .WithArguments(["-c", gdbInjectCommand])
                    .ExecuteAsync();

                // Obtain handle to injected module though thread exit code
                var moduleName = Path.GetFileName(modulePath);
                var module = Process.GetProcessById(pid).Modules.Cast<ProcessModule>().SingleOrDefault(m => m.ModuleName == moduleName);
                if (module == null)
                {
                    // Error while obtaining base address for loaded module
                    // FIXME [#16]: logging
                    return GenericSafeHandle.Invalid;
                }

                return new GenericSafeHandle(() => module.BaseAddress, _ => true, ownsHandle: false);
            }
            catch (Exception)
            {
                // Error while obtaining base address for loaded module
                // FIXME [#16]: logging
                return GenericSafeHandle.Invalid;
            }
        }

        private static async Task<bool> IsGdbInstalled()
        {
            try
            {
                await Cli.Wrap(ShellExecutable)
                    .WithArguments(["-c", GdbTestCommand])
                    .WithValidation(CommandResultValidation.ZeroExitCode)
                    .ExecuteAsync();

                return true;
            }
            catch (Exception)
            {
                // Error while accessing gdb command
                // FIXME [#16]: logging
                return false;
            }
        }
    }
}
