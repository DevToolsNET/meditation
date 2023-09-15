namespace Meditation.MetadataLoaderService.Models
{
    public record struct MetadataToken(int Value);
    public record struct AssemblyToken(int Value);
    public record struct ModuleToken(int Value);
    public record struct TypeDefinitionToken(int Value);
    public record struct MethodDefinitionToken(int Value);
}
