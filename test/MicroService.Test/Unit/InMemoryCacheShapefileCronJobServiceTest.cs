using MicroService.Service.Configuration;
using MicroService.WebApi.Services;
using MicroService.WebApi.Services.Cron;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Xunit;

namespace MicroService.Test.Unit
{
    public sealed class InMemoryCacheShapefileCronJobServiceTest : IDisposable
    {
        private readonly string _root = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        private readonly MemoryCache _cache = new(new MemoryCacheOptions());
        private readonly RecordingLogger _logger = new();
        private readonly CronJobServiceHealthCheck _health = new();
        private readonly List<InMemoryCacheShapefileCronJobService> _services = new();

        public InMemoryCacheShapefileCronJobServiceTest()
        {
            Directory.CreateDirectory(_root);
        }

        [Fact]
        public async Task DoWork_LoadsFeatures_RefreshesCache_AndRemovesDeletedFile()
        {
            WriteShape();
            using var service = CreateService(_root);
            await service.DoWork(CancellationToken.None);

            var original = Assert.IsType<IReadOnlyCollection<Feature>>(_cache.Get("BoroughBoundaries"), exactMatch: false);
            Assert.Equal("sample", Assert.Single(original).Attributes["name"]);

            await service.DoWork(CancellationToken.None);
            Assert.NotSame(original, _cache.Get("BoroughBoundaries"));

            File.Delete(ShapePath + ".dbf");
            await service.DoWork(CancellationToken.None);
            Assert.Null(_cache.Get("BoroughBoundaries"));
        }

        [Fact]
        public async Task DoWork_MissingFiles_DoesNotPopulateCache()
        {
            using var service = CreateService(_root);
            await service.DoWork(CancellationToken.None);
            Assert.Null(_cache.Get("BoroughBoundaries"));
        }

        [Fact]
        public async Task DoWork_MissingConfiguration_ThrowsHelpfulError()
        {
            using var service = CreateService(null);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DoWork(CancellationToken.None));
            Assert.Contains("ShapeConfiguration:ShapeRootDirectory", exception.Message);
        }

        [Fact]
        public async Task DoWork_Cancelled_DoesNotReadFiles()
        {
            using var service = CreateService(null);
            await service.DoWork(new CancellationToken(true));
            Assert.Null(_cache.Get("BoroughBoundaries"));
        }

        [Fact]
        public async Task StartAndStop_UpdateReadiness_AndLeaveSharedCacheUsable()
        {
            using var service = CreateService(_root);
            await service.StartAsync(CancellationToken.None);
            Assert.True(_health.StartupTaskCompleted);
            await service.StopAsync(CancellationToken.None);
            await service.DoWork(CancellationToken.None);
            _cache.Set("other-service", 1);
            Assert.Equal(1, _cache.Get<int>("other-service"));
        }

        [Fact]
        public async Task FileChange_RefreshesCache()
        {
            WriteShape();
            using var service = CreateService(_root);
            await service.DoWork(CancellationToken.None);
            var refreshed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _logger.OnLog = (level, message) =>
            {
                if (level == LogLevel.Information && message.Contains("[CacheRefresh]"))
                    refreshed.TrySetResult();
            };

            File.SetLastWriteTimeUtc(ShapePath + ".dbf", DateTime.UtcNow.AddSeconds(-10));

            await refreshed.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
            Assert.NotNull(_cache.Get("BoroughBoundaries"));
        }

        [Fact]
        public async Task FileChange_ReadFailure_IsLogged()
        {
            WriteShape();
            using var service = CreateService(_root);
            await service.DoWork(CancellationToken.None);
            var logged = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            _logger.OnLog = (level, message) =>
            {
                if (level == LogLevel.Error && message.Contains("[FileChangedError]"))
                    logged.TrySetResult(message);
            };
            File.Delete(ShapePath + ".shp");
            File.SetLastWriteTimeUtc(ShapePath + ".dbf", DateTime.UtcNow.AddSeconds(-10));

            var errorMessage = await logged.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
            Assert.Contains("nybb.dbf", errorMessage);
        }

        private string ShapePath => Path.Join(_root, "Borough_Boundaries", "NAD83", "nybb");

        private void WriteShape()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ShapePath)!);
            var feature = new Feature(new Point(1, 2), new AttributesTable { { "name", "sample" } });
            var writer = new ShapefileDataWriter(ShapePath, new GeometryFactory())
            {
                Header = ShapefileDataWriter.GetHeader(feature, 1)
            };
            writer.Write(new[] { feature });
        }

        private InMemoryCacheShapefileCronJobService CreateService(string? root)
        {
            var service = new InMemoryCacheShapefileCronJobService(
                new ScheduleConfig { CronExpression = "0 * * * *", TimeZoneInfo = TimeZoneInfo.Utc },
                _cache, _health,
                Options.Create(new ApplicationOptions { ShapeConfiguration = new ShapeConfiguration { ShapeRootDirectory = root } }),
                _logger);
            _services.Add(service);
            return service;
        }

        private sealed class RecordingLogger : ILogger<InMemoryCacheShapefileCronJobService>
        {
            public Action<LogLevel, string>? OnLog { get; set; }
            public bool IsEnabled(LogLevel logLevel) => true;
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
                Exception? exception, Func<TState, Exception?, string> formatter) =>
                OnLog?.Invoke(logLevel, formatter(state, exception));
        }

        public void Dispose()
        {
            foreach (var service in _services)
                service.Dispose();
            _cache.Dispose();
            Directory.Delete(_root, true);
        }
    }
}
