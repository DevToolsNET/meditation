using System;

namespace Meditation.PatchLibrary
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class MeditationPatchTypeTargetAttribute : Attribute
    {
        public string TypeFullName { get; }

        public MeditationPatchTypeTargetAttribute(string typeFullName)
        {
            TypeFullName = typeFullName;
        }
    }
}
