using System;

namespace Meditation.PatchLibrary
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class MeditationPatchMethodTargetAttribute : Attribute
    {
        public string Name { get; }
        public bool IsStatic { get; }
        public int ParametersCount { get; }

        public MeditationPatchMethodTargetAttribute(string name, bool isStatic, int parametersCount)
        {
            Name = name;
            IsStatic = isStatic;
            ParametersCount = parametersCount;
        }
    }
}
