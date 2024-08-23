using System;
using System.Reflection;

namespace Meditation.PatchLibrary
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class MeditationPatchAssemblyTargetAttribute : Attribute
    {
        public AssemblyName AssemblyFullName { get; }

        public MeditationPatchAssemblyTargetAttribute(string assemblyFullName)
        {
            AssemblyFullName = new AssemblyName(assemblyFullName);
        }
    }
}
