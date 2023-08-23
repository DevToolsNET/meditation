﻿using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Meditation.Common.Models;

namespace Meditation.Common.Services
{
    public interface IAttachableProcessListProvider
    {
        ProcessType ProviderType { get; }
        Task<ImmutableArray<ProcessInfo>> GetAttachableProcessesAsync(CancellationToken ct);

        void Refresh();
    }
}
