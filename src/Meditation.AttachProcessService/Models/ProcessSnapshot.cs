using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Interfaces;
using System;
using System.Collections.Generic;

namespace Meditation.AttachProcessService.Models
{
    internal class ProcessSnapshot : IProcessSnapshot
    {
        private readonly IDataTarget _dataTarget;
        private bool _isDisposed;

        public ProcessSnapshot(int processId, IDataTarget dataTarget)
        {
            ProcessId = processId;
            _dataTarget = dataTarget;
        }

        public int ProcessId { get; }

        public IEnumerable<ModuleInfo> GetModules() => _dataTarget.EnumerateModules();

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _dataTarget.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
