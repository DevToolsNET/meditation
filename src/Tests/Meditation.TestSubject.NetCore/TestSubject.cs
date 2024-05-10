using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Meditation.TestInjectionTarget;

namespace Meditation.TestSubject.NetCore
{
    public static class TestSubject
    {
        public const string SynchronizationHandleName = "/meditation/tests-ipc-signal";

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void Method()
        {
            Console.WriteLine("Called method");
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void Main()
        {
            var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, SynchronizationHandleName);
            TestClass.Init();

            while (!waitHandle.WaitOne(TimeSpan.FromSeconds(value: 1)))
                Method();
        }
    }
}