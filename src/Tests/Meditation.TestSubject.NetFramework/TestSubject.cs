using System;
using System.Threading;

namespace Meditation.TestSubject.NetFramework
{
    public static class TestSubject
    {
        public const string SynchronizationHandleName = "/meditation/tests-ipc-signal";

        public static void Method()
        {
            Console.WriteLine("Called method");
        }

        public static void Main()
        {
            var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, SynchronizationHandleName);
            while (!waitHandle.WaitOne(TimeSpan.FromSeconds(value: 5)))
                Method();
        }
    }
}