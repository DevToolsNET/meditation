using Avalonia.Controls;
using AvaloniaEdit.TextMate;
using Meditation.UI.ViewModels.IDE;
using System;
using TextMateSharp.Grammars;

namespace Meditation.UI.Views.IDE
{
    public partial class IdeTextEditorView : UserControl
    {
        public IdeTextEditorView()
        {
            InitializeComponent();
            SetupSyntaxHighlighting();
        }

        private void SetupSyntaxHighlighting()
        {
            var registryOptions = new RegistryOptions(ThemeName.Light);
            var textMateInstallation = TextEditor.InstallTextMate(registryOptions);
            textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".cs").Id));
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is not IdeTextEditorViewModel textEditorViewModel)
                throw new ArgumentException($"Expected DataContext of type {nameof(IdeTextEditorViewModel)}");

            TextEditor.Text = textEditorViewModel.Text;
            TextEditor.TextChanged += (_, _) => textEditorViewModel.Text = TextEditor.Text;
            textEditorViewModel.TextChanged += newText =>
            {
                if (newText != TextEditor.Text)
                    TextEditor.Text = newText;
            };

            TextEditor.IsReadOnly = !textEditorViewModel.Enabled;
            textEditorViewModel.EnabledChanged += newValue =>
            {
                TextEditor.IsReadOnly = !newValue;
            };
        }
    }
}
