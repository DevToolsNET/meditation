using CommunityToolkit.Mvvm.Input;
using Meditation.MetadataLoaderService.Models;
using Meditation.UI.ViewModels;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Threading.Tasks;

namespace Meditation.UI.Controllers
{
    public partial class DevelopmentEnvironmentController
    {
        private readonly IAvaloniaDialogService _dialogService;
        private readonly IWorkspaceContext _compilationContext;

        public DevelopmentEnvironmentController(
            IWorkspaceContext compilationContext, 
            IAvaloniaDialogService dialogService)
        {
            _dialogService = dialogService;
            _compilationContext = compilationContext;
        }

        [RelayCommand]
        public async Task CreateWorkspace(MetadataBrowserViewModel metadataBrowserViewModel)
        {
            if (metadataBrowserViewModel.SelectedItem is not MethodMetadataEntry method)
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard(
                    title: "No method was selected",
                    text: "Unable to create a new workspace because no method was selected.",
                    @enum: ButtonEnum.Ok);
                await messageBox.ShowAsync();
                return;
            }

            _compilationContext.CreateWorkspace(method);
        }

        [RelayCommand]
        public async Task DestroyWorkspace()
        {
            if (_compilationContext.Method is not { } method)
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard(
                    title: "No workspace is active",
                    text: "Unable to destroy workspace as there is no editor active.",
                    @enum: ButtonEnum.Ok);
                await messageBox.ShowAsync();
                return;
            }

            _compilationContext.DestroyWorkspace();
        }
    }
}
