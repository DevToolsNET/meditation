using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Meditation.MetadataLoaderService.Models;
using Meditation.UI.Utilities;
using System.Linq;
using Meditation.MetadataLoaderService;

namespace Meditation.UI.ViewModels
{
    public partial class MetadataBrowserViewModel : ViewModelBase
    {
        [ObservableProperty] private FilterableObservableCollection<AssemblyMetadataEntry> _assemblies;
        [ObservableProperty] private string? _metadataNameFilter;
        private readonly IUserInterfaceEventsConsumer _eventsConsumer;

        public MetadataBrowserViewModel(IMetadataLoader metadataLoader, IUserInterfaceEventsConsumer eventsConsumer)
        {
            _eventsConsumer = eventsConsumer;
            _assemblies = new FilterableObservableCollection<AssemblyMetadataEntry>(Enumerable.Empty<AssemblyMetadataEntry>());

            eventsConsumer.AssemblyLoadRequested += path =>
            {
                var assemblyMetadata = metadataLoader.LoadMetadataFromAssembly(path);
                AddAssembly(assemblyMetadata);
            };
        }

        public void AddAssembly(AssemblyMetadataEntry entry)
        {
            Assemblies.Add(entry);
        }

        [RelayCommand]
        public void FilterMetadata()
        {
            Assemblies.ApplyFilter(p => MetadataNameFilter == null || p.Name.Contains(MetadataNameFilter));
        }
    }
}
