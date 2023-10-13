using System;

namespace Meditation.MetadataLoaderService.Models
{
    public readonly record struct MetadataToken
    {
        public int Value { get; }

        public MetadataToken(int value)
        {
            EnsureTokenValueIsValid(value);
            Value = value;
        }

        public static void EnsureTokenValueIsValid(int value)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be positive.");
        }
    }

    public readonly record struct ModuleToken
    {
        public int Value { get; }

        public ModuleToken(int value)
        {
            MetadataToken.EnsureTokenValueIsValid(value);
            Value = value;
        }
    }

    public readonly record struct AssemblyToken
    {
        public int Value { get; }

        public AssemblyToken(int value)
        {
            MetadataToken.EnsureTokenValueIsValid(value);
            Value = value;
        }
    }

    public readonly record struct TypeDefinitionToken
    {
        public int Value { get; }

        public TypeDefinitionToken(int value)
        {
            MetadataToken.EnsureTokenValueIsValid(value);
            Value = value;
        }
    }

    public readonly record struct MethodDefinitionToken
    {
        public int Value { get; }

        public MethodDefinitionToken(int value)
        {
            MetadataToken.EnsureTokenValueIsValid(value);
            Value = value;
        }
    }
}