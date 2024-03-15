using System;
using System.IO;

namespace Meditation.Bootstrap.Managed.Utils
{
    internal sealed class Logger : IDisposable
    {
        private readonly StreamWriter? _loggingStream;
        private bool _disposed;

        public Logger(string filename)
        {
            var directory = Path.GetDirectoryName(filename);
            if (directory != null && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            _loggingStream = new StreamWriter(filename, append: true);
        }

        public void LogInfo(string message)
            => _loggingStream?.WriteLine(FormatMessage("INF", message));

        public void LogError(string message)
            => _loggingStream?.WriteLine(FormatMessage("ERR", message));

        private static string FormatMessage(string level, string input)
            => $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [{level}]: {input}";

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _loggingStream?.Dispose();
        }
    }
}
