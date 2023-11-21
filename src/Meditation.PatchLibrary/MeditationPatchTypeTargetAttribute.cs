using System;

namespace Meditation.PatchLibrary
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class MeditationPatchTypeTargetAttribute : Attribute
    {
        public string TypeFullName { get; }

        public MeditationPatchTypeTargetAttribute(string typeFullName)
        {
            TypeFullName = typeFullName;
        }
    }
}
