using System;
using System.Diagnostics;

namespace Meditation.Common.Models
{
    public record ProcessInfo(int Id, string Name, ProcessType Type, Lazy<string?> CommandLineArguments, Process Internal)
    {
        public ProcessInfo(Process process, ProcessType type, Lazy<string?> commandLineArguments)
            : this(process.Id, process.ProcessName, type, commandLineArguments, process)
        {

        }

        public override string ToString()
            => $"[{Id}] {Name}";
    }
}
