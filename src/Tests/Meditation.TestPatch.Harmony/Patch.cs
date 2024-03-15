using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;

namespace Meditation.TestPatch.Harmony
{
    [HarmonyPatch]
    public class Patch
    {
        [HarmonyTargetMethod]
        public static MethodBase GetTargetMethod()
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Single(a => a.GetName().Name.Equals("Meditation.TestInjectionTarget"))
                .GetTypes()
                .Single(t => t.FullName == "Meditation.TestInjectionTarget.TestClass")
                .GetMethod("TestMethod")
                   ?? throw new ArgumentException("Could not find target method");
        }

        [HarmonyPostfix]
        public static void CustomPostfix(ref int __result)
        {
            __result = 42;
        }
    }
}
