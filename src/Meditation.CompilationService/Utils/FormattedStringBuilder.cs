using System;
using System.Text;

namespace Meditation.CompilationService.Utils
{
    public class FormattedStringBuilder
    {
        private int _tabIndex;
        private StringBuilder _sb = new StringBuilder();

        public FormattedStringBuilder(int initialIndentationLevel = 0)
        {
            _tabIndex = initialIndentationLevel;
            EnsureValidIndentationLevel();
        }

        public void IncreaseIndentation()
        {
            _tabIndex++;
            EnsureValidIndentationLevel();
        }

        public void DecreaseIndentation()
        {
            _tabIndex--;
            EnsureValidIndentationLevel();
        }

        public void AppendLine(string line = "")
        {
            if (line != string.Empty)
                _sb.Append('\t', repeatCount: _tabIndex);
            _sb.AppendLine(line);
        }

        public override string ToString()
            => _sb.ToString();

        private void EnsureValidIndentationLevel()
        {
            if (_tabIndex < 0)
                throw new InvalidOperationException("Indentation can not have negative value.");
        }
    }
}