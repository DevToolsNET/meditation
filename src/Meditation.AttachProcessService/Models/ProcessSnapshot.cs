using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Interfaces;
using System.Collections.Generic;

namespace Meditation.AttachProcessService.Models
{
    internal class ProcessSnapshot : IProcessSnapshot
    {
        private readonly IDataTarget _dataTarget;
        private bool _isDisposed;

        public ProcessSnapshot(ProcessId processId, IDataTarget dataTarget)
        {
            ProcessId = processId;
            _dataTarget = dataTarget;
        }

        public ProcessId ProcessId { get; }

        public IEnumerable<ModuleInfo> EnumerateModules() => _dataTarget.EnumerateModules();

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _dataTarget.Dispose();
        }
    }
}
