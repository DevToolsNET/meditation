using Meditation.MetadataLoaderService.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Xunit;

namespace Meditation.MetadataLoaderService.Tests
{
    public class MetadataLoaderTests : TestsBase
    {
        [Fact]
        public void MetadataLoader_BuildMetadataHierarchy()
        {
            // Prepare
            const string methodName = nameof(MetadataLoader_BuildMetadataHierarchy);
            var typeFullName = typeof(MetadataLoaderTests).FullName;
            var assemblyLocation = typeof(MetadataLoaderTests).Assembly.Location;
            var loader = ServiceProvider.GetRequiredService<IMetadataLoader>();

            // Act
            var assemblyMetadata = loader.LoadMetadataFromAssembly(assemblyLocation);
            var moduleMetadata = assemblyMetadata.Children.FirstOrDefault();
            var typeMetadata = moduleMetadata?.Children.FirstOrDefault(child => child is TypeMetadataEntry typeEntry && typeEntry.FullName == typeFullName);
            var methodMetadata = typeMetadata?.Children.FirstOrDefault(child => child is MethodMetadataEntry { Name: methodName });

            // Assert
            Assert.NotNull(assemblyMetadata);
            Assert.NotNull(moduleMetadata);
            Assert.NotNull(typeMetadata);
            Assert.NotNull(methodMetadata);
        }
    }
}