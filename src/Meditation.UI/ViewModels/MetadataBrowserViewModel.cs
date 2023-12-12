using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Meditation.MetadataLoaderService.Models;
using Meditation.UI.Utilities;
using System.Linq;

namespace Meditation.UI.ViewModels
{
    public partial class MetadataBrowserViewModel : ViewModelBase
    {
        [ObservableProperty] private FilterableObservableCollection<MetadataEntryBase> _items;
        [ObservableProperty] private string? _metadataNameFilter;
        [ObservableProperty] private bool? _isLoadingData;
        private readonly IAttachedProcessContext _attachedProcessContext;

        public MetadataBrowserViewModel(IAttachedProcessContext attachedProcessContext)
        {
            _attachedProcessContext = attachedProcessContext;
            _items = new FilterableObservableCollection<MetadataEntryBase>(Enumerable.Empty<MetadataEntryBase>());
            RegisterEventHandlers();
        }

        private void RegisterEventHandlers()
        {
            // Set loading flag on process attaching
            _attachedProcessContext.ProcessAttaching += _ => IsLoadingData = true;

            // Clear loading flag on process attached
            _attachedProcessContext.ProcessAttached += _ => IsLoadingData = false;

            // Add assembly on process assembly loaded
            _attachedProcessContext.AssemblyOrNetModuleLoaded += (_, metadata) => Items.Add(metadata);

            // Clear data on process detach
            _attachedProcessContext.ProcessDetached +=
                _ => Items = new FilterableObservableCollection<MetadataEntryBase>(Enumerable.Empty<MetadataEntryBase>());
        }

        [RelayCommand]
        public void FilterMetadata()
        {
            Items.ApplyFilter(p => MetadataNameFilter == null || p.Name.Contains(MetadataNameFilter));
        }
    }
}
