using CliWrap;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Meditation.AttachProcessService.Services.Linux
{
    internal class LinuxProcessArchitectureProvider : IProcessArchitectureProvider
    {
        private const string Pattern64Bit = "x86-64";
        private const string Pattern32Bit = "80386";

        public async Task<Architecture?> TryGetProcessArchitectureAsync(Process process, CancellationToken ct)
        {
            var stdoutBuffer = new StringBuilder();
            var stderrBuffer = new StringBuilder();
            var command = Cli.Wrap("/bin/sh")
                .WithArguments(["-c", $"file -L /proc/{process.Id}/exe"])
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdoutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stderrBuffer))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync(ct);

            var result = await command;
            var stdout = stdoutBuffer.ToString();
            var stderr = stderrBuffer.ToString();
            if (!result.IsSuccess || stderr.Length != 0)
            {
                // FIXME [#16]: add logging for stderr and error code of process
                return null;
            }

            if (stdout.Contains(Pattern64Bit, StringComparison.Ordinal))
                return Architecture.X64;
            if (stdout.Contains(Pattern32Bit, StringComparison.Ordinal))
                return Architecture.X86;

            // FIXME [#16]: unknown architecture
            return null;
        }
    }
}
