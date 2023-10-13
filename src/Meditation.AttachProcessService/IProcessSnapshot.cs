using Meditation.AttachProcessService.Models;
using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;

namespace Meditation.AttachProcessService
{
    public interface IProcessSnapshot : IDisposable
    {
        public ProcessId ProcessId { get; }

        public IEnumerable<ModuleInfo> EnumerateModules();
    }
}