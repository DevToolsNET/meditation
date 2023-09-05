using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;

namespace Meditation.AttachProcessService
{
    public interface IProcessSnapshot : IDisposable
    {
        public int ProcessId { get; }

        public IEnumerable<ModuleInfo> GetModules();
    }
}