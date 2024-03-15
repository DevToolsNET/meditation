using System;
using System.Runtime.CompilerServices;

// Note: If you want to change anything in this file, make sure tests patch is update accordingly
namespace Meditation.TestInjectionTarget
{
    public static class TestClass
    {
        public static void Init()
        {

        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static int TestMethod()
        {
            Console.WriteLine("DO");
            _ = new object();
            Console.WriteLine("NOT");
            _ = new object();
            Console.WriteLine("INLINE");
            _ = new object();
            Console.WriteLine("THIS");
            _ = new object();
            Console.WriteLine("!!!");
            _ = new object();
            return 1;
        }
    }
}
