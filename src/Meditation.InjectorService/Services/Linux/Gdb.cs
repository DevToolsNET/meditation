using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CliWrap;
using Meditation.Interop;
using Meditation.Interop.Linux;

namespace Meditation.InjectorService.Services.Linux;

internal static class Gdb
{
    private const string ShellExecutable = "/bin/sh";
    private const string GdbTestCommand = "gdb --version";
    
    public static async Task<bool> IsInstalled()
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
    
    public static async Task<SafeHandle> TryInjectModule(int pid, string modulePath)
    {
        try
        {
            // Inject module into remote process
            const int flags = DynamicLinking.DLOPEN_RTLD_NOW | DynamicLinking.DLOPEN_RTLD_GLOBAL;
            var gdbInjectModuleScript = BuildInjectModuleGdbScript(pid, modulePath, flags);
            var stdoutBuffer = new StringBuilder();
            
            var result = await Cli.Wrap(ShellExecutable)
                .WithArguments(["-c", gdbInjectModuleScript])
                .WithValidation(CommandResultValidation.None)
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdoutBuffer))
                .ExecuteAsync();

            // Obtain handle to injected module though thread exit code
            var moduleName = Path.GetFileName(modulePath);
            var module = Process.GetProcessById(pid).Modules.Cast<ProcessModule>().SingleOrDefault(m => m.ModuleName == moduleName);
            
            if (!result.IsSuccess || module == null)
            {
                // FIXME [#16]: logging
                // Error while obtaining base address for loaded module
                // Log stdout from gdb execution for debugging purposes
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

    public static async Task<uint?> TryExecuteFunction(int pid, string functionName, string argument)
    {
        try
        {
            // Execute function in remote process
            var stdoutBuffer = new StringBuilder();
            var gdbExecuteFunctionScript = BuildExecuteFunctionGdbScript(pid, functionName, argument);
            var result = await Cli.Wrap(ShellExecutable)
                .WithArguments(["-c", $"{gdbExecuteFunctionScript}"])
                .WithValidation(CommandResultValidation.None)
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdoutBuffer))
                .ExecuteAsync();
            
            // Parse output to obtain return code
            var stdout = stdoutBuffer.ToString();
            var evaluationLine = stdout.Split(Environment.NewLine).SingleOrDefault(line => line.StartsWith("$1"));
            if (!result.IsSuccess || evaluationLine == null)
            {
                // Could not find expression evaluation line in gdb output
                // FIXME [#16]: logging
                return null;
            }
            var rawExitCode = evaluationLine.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Last();
            if (!uint.TryParse(rawExitCode, out var exitCode))
            {
                // Could not parse exit code from gdb output
                // FIXME [#16]: logging
                return null;
            }

            return exitCode;
        }
        catch (Exception)
        {
            // Error while executing method in remote process
            // FIXME [#16]: logging
            return null;
        }
    }

    private static string BuildInjectModuleGdbScript(int pid, string modulePath, int flags)
    {
        return string.Join(" ", new[]
        {
            $"gdb -p {pid}",
            $"--batch",
            $"-ex 'print dlopen(\"{modulePath}\", {flags})'",
            $"-ex 'print dlerror()'"
        });
    }
    
    private static string BuildExecuteFunctionGdbScript(int pid, string functionName, string argument)
    {
        // Convert argument string to null-terminated uint16_t initializer list: { 0x006E, 0x0061, 0x0074, ... }
        var bytes = Encoding.Unicode.GetBytes(argument + '\0');
        var utf16Initializer = string.Join(",", Enumerable.Range(0, bytes.Length / 2)
            .Select(i => BitConverter.ToUInt16(bytes, i * 2))
            .Select(w => $"0x{w:X4}"));

        var wideCharCount = bytes.Length / 2;
        return string.Join(" ", new[]
        {
            $"gdb -p {pid}",
            $"--batch",
            $"-ex 'set $arr = malloc({wideCharCount} * 2)'",
            $"-ex 'set {{unsigned short[{wideCharCount}]}} $arr = {{{utf16Initializer}}}'",
            $"-ex 'print (unsigned int){functionName}((unsigned short*)$arr)'",
            $"-ex 'call free($arr)'"
        });
    }
}