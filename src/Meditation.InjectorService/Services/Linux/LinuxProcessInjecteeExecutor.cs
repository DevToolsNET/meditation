using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Meditation.InjectorService.Services.Linux;

internal class LinuxProcessInjecteeExecutor : IProcessInjecteeExecutor
{
    public async Task<uint> ExecuteExportedMethod(int pid, string modulePath, SafeHandle injectedModuleHandle, string exportedMethodName, string argument)
    {
        if (await TryExecuteExportedMethod(pid, modulePath, injectedModuleHandle, exportedMethodName, argument) is not { } exitCode)
            throw new Exception($"Could not call exported symbol {exportedMethodName} with argument {argument} in PID = {pid}.");

        return exitCode;
    }
    
    public async Task<uint?> TryExecuteExportedMethod(
        int pid,
        string modulePath,
        SafeHandle _,
        string exportedMethodName,
        string argument)
    {
        if (!await Gdb.IsInstalled())
        {
            // Could not find usable gdb
            // FIXME [#16]: logging
            return null;
        }

        if (await Gdb.TryExecuteFunction(pid, exportedMethodName, argument) is not { } exitCode)
        {
            // Could not execute function in remote process
            // FIXME [#16]: logging
            return null;
        }
        
        return exitCode;
    }
}