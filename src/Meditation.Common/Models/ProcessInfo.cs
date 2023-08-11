using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Meditation.Common.Models
{
    public record ProcessInfo(
        int Id, 
        string Name, 
        ProcessType Type, 
        Lazy<string?> CommandLineArguments, 
        Lazy<Architecture?> Architecture, 
        Process Internal)
    {
        public ProcessInfo(Process process, ProcessType type, Lazy<string?> commandLineArguments, Lazy<Architecture?> architecture)
            : this(process.Id, process.ProcessName, type, commandLineArguments, architecture, process)
        {

        }

        public override string ToString()
            => $"[{Id}] {Name}";
    }
}
