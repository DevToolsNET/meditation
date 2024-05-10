using System;

namespace Meditation.PatchLibrary
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class MeditationPatchAssemblyTargetAttribute : Attribute
    {
        public string AssemblyFullName { get; }

        public MeditationPatchAssemblyTargetAttribute(string assemblyFullName)
        {
            AssemblyFullName = assemblyFullName;
        }
    }
}
