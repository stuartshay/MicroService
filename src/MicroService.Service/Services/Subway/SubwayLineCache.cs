using Microsoft.Extensions.Caching.Memory;
using System;

namespace MicroService.Service.Services.Subway
{
    /// <summary>
    /// The shaped subway lines, one entry per (services, borough, simplify, stations, radius,
    /// stub) request, and the borough outlines they are clipped to. A cache of its own, so that
    /// its size limit bounds what many different requests can hold without touching the shared
    /// shape cache. Entry sizes are approximate bytes.
    /// </summary>
    public sealed class SubwayLineCache : IDisposable
    {
        public const long SizeLimitBytes = 64L * 1024 * 1024;

        public MemoryCache Cache { get; } = new(new MemoryCacheOptions { SizeLimit = SizeLimitBytes });

        public void Dispose() => Cache.Dispose();
    }
}
