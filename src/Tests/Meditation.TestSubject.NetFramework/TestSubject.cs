using Meditation.TestInjectionTarget;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Meditation.TestSubject.NetFramework
{
    public static class TestSubject
    {
        public const string SynchronizationHandleName = "/meditation/tests-ipc-signal";

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void Method()
        {
            Console.WriteLine("Called method");
        }

        public static void Main()
        {
            var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, SynchronizationHandleName);
            TestClass.Init();

            while (!waitHandle.WaitOne(TimeSpan.FromSeconds(value: 5)))
                Method();
        }
    }
}