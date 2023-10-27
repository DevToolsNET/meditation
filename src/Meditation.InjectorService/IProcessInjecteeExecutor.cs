﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Meditation.InjectorService
{
    public interface IProcessInjecteeExecutor
    {
        bool TryExecuteExportedMethod(int pid, string modulePath, SafeHandle injectedModuleHandle, string exportedMethodName,
            [NotNullWhen(returnValue: true)] out uint? returnCode);
    }
}
