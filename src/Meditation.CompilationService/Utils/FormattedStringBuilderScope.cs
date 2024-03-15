using System;

namespace Meditation.CompilationService.Utils
{
    public class FormattedStringBuilderIndentationScope : IDisposable
    {
        private readonly FormattedStringBuilder _sb;
        private bool _isDisposed;

        public FormattedStringBuilderIndentationScope(FormattedStringBuilder sb)
        {
            _sb = sb;
            _sb.AppendLine("{");
            _sb.IncreaseIndentation();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _sb.DecreaseIndentation();
            _sb.AppendLine("}");
        }
    }
}