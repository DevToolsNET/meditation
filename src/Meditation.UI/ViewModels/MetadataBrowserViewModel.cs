using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Meditation.MetadataLoaderService.Models;
using Meditation.UI.Utilities;
using System.Linq;
using Avalonia.Controls.Shapes;
using Meditation.AttachProcessService;
using Meditation.MetadataLoaderService;

namespace Meditation.UI.ViewModels
{
    public partial class MetadataBrowserViewModel : ViewModelBase
    {
        [ObservableProperty] private FilterableObservableCollection<AssemblyMetadataEntry> _assemblies;
        [ObservableProperty] private string? _metadataNameFilter;
        [ObservableProperty] private bool? _isLoadingData;
        private readonly IAttachedProcessProvider _attachedProcessProvider;
        private readonly IMetadataLoader _metadataLoader;

        public MetadataBrowserViewModel(IMetadataLoader metadataLoader, IAttachedProcessProvider attachedProcessProvider)
        {
            _metadataLoader = metadataLoader;
            _attachedProcessProvider = attachedProcessProvider;
            _assemblies = new FilterableObservableCollection<AssemblyMetadataEntry>(Enumerable.Empty<AssemblyMetadataEntry>());
            RegisterEventHandlers();
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

        private void RegisterEventHandlers()
        {
            _attachedProcessProvider.AttachedProcessChanged += snapshot =>
            {
                ClearPreviousProcessData();

                if (snapshot != null) 
                    HandleProcessAttach(snapshot);
                else
                    HandleProcessDetach();
            };
        }

        private void ClearPreviousProcessData()
        {
            Assemblies = new FilterableObservableCollection<AssemblyMetadataEntry>(Enumerable.Empty<AssemblyMetadataEntry>());
        }

        private void HandleProcessAttach(IProcessSnapshot snapshot)
        {
            IsLoadingData = true;

            var modules = snapshot.GetModules();
            foreach (var module in modules.Where(m => m.IsManaged).OrderBy(m => System.IO.Path.GetFileName(m.FileName)))
            {
                try
                {
                    var assemblyMetadata = _metadataLoader.LoadMetadataFromAssembly(module.FileName);
                    AddAssembly(assemblyMetadata);
                }
                catch (Exception a)
                {
                    // FIXME: add logging
                    throw;
                }
            }

            IsLoadingData = false;
        }

        private void HandleProcessDetach()
        {
            // Nothing to load
        }
    }
}
