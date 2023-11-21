using System;

namespace Meditation.PatchLibrary
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class MeditationPatchAssemblyTargetAttribute : Attribute
    {
        public string AssemblyFullName { get; }

        public MeditationPatchAssemblyTargetAttribute(string assemblyFullName)
        {
            AssemblyFullName = assemblyFullName;
        }
    }
}
