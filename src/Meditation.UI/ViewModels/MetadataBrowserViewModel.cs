using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Meditation.MetadataLoaderService.Models;

namespace Meditation.UI.ViewModels
{
    public partial class MetadataBrowserViewModel : ViewModelBase
    {
        [ObservableProperty] private ObservableCollection<AssemblyMetadataEntry> _assemblies;

        public MetadataBrowserViewModel()
        {
            _assemblies = new ObservableCollection<AssemblyMetadataEntry>();
        }

        public void AddAssembly(AssemblyMetadataEntry entry)
            => Assemblies.Add(entry);
    }
}
