namespace Meditation.InjectorService
{
    public interface IProcessInjector
    {
        bool TryInjectModuleToProcess(int pid, string assemblyPath, out IntPtr remoteModuleHandle);
        bool TryExecuteStaticMethodInDefaultProcessAppDomain(int pid, string assemblyPath, string fullTypeName, string methodName, string arg, out int returnCode);
    }
}
