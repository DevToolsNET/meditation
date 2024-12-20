using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Meditation.MetadataLoaderService.Models;
using Meditation.PatchingService;
using Meditation.PatchingService.Models;
using Meditation.UI.Controllers;
using Meditation.UI.Utilities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.UI.ViewModels
{
    public partial class PatchesBrowserViewModel : ViewModelBase
    {
        [ObservableProperty] private FilterableObservableCollection<MetadataEntryBase> _items;
        [ObservableProperty] private string? _patchesNameFilter;
        [ObservableProperty] private bool _isLoadingData;
        [ObservableProperty] private bool _hasData;
        [ObservableProperty] private PatchInfo? _selectedItem;
        private readonly PatchProcessController _patchProcessController;
        private readonly IAttachedProcessContext _attachedProcessContext;
        private readonly IPatchListProvider _patchListProvider;
        private readonly IPatchViewModelBuilder _patchViewModelBuilder;

        public PatchesBrowserViewModel(
            PatchProcessController patchProcessController,
            IAttachedProcessContext attachedProcessContext,
            IPatchViewModelBuilder patchViewModelBuilder, 
            IPatchListProvider patchProvider)
        {
            _patchProcessController = patchProcessController;
            _attachedProcessContext = attachedProcessContext;
            _patchViewModelBuilder = patchViewModelBuilder;
            _patchListProvider = patchProvider;
            _items = new FilterableObservableCollection<MetadataEntryBase>(Enumerable.Empty<MetadataEntryBase>());
            RegisterEventHandlers();
        }

        private void RegisterEventHandlers()
        {
            // Clear loading flag on process attached
            _attachedProcessContext.ProcessAttached += _ => Refresh();

            // Clear data on process detach
            _attachedProcessContext.ProcessDetached += _ =>
            {
                Items.View.Clear();
                HasData = false;
            };
        }

        [RelayCommand]
        public Task Apply(string patchAssemblyPath, CancellationToken ct)
        {
            return _patchProcessController.ApplyPatch(patchAssemblyPath, ct);
        }

        [RelayCommand]
        public Task Reverse(string patchAssemblyPath, CancellationToken ct)
        {
            return _patchProcessController.ReversePatch(patchAssemblyPath, ct);
        }

        [RelayCommand]
        public void Refresh()
        {
            IsLoadingData = true;
            _patchListProvider.Reload();
            Items.View.Clear();
            foreach (var item in _patchListProvider
                         .GetAllPatches()
                         .Select(kv => _patchViewModelBuilder.Build(kv.Key, kv.Value))
                         .OrderBy(e => e.FullName))
            {
                Items.Add(item);
            }
            IsLoadingData = false;
            HasData = true;
        }

        [RelayCommand]
        public void FilterPatches()
        {
            Items.ApplyFilter(p => PatchesNameFilter == null || p.Name.Contains(PatchesNameFilter));
        }
    }
}
