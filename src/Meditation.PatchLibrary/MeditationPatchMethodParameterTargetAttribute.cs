using System;

namespace Meditation.PatchLibrary
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class MeditationPatchMethodParameterTargetAttribute : Attribute
    {
        public string TypeFullName { get; }
        public int Index { get; }

        public MeditationPatchMethodParameterTargetAttribute(int index, string typeFullName)
        {
            Index = index;
            TypeFullName = typeFullName;
        }
    }
}
