using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Meditation.TestInjectionTarget;

namespace Meditation.TestSubject.NetCore
{
    public static class TestSubject
    {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void Method()
        {
            Console.WriteLine("Called method");
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void Main()
        {
            TestClass.Init();

            while (Console.In.Peek() == -1)
            {
                Method();
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
    }
}