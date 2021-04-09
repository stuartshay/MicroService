using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MicroService.Service.Configuration;
using MicroService.Service.Extensions;
using MicroService.Service.Helpers;
using MicroService.Service.Models.Enum;
using MicroService.WebApi.Services.Cron;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace MicroService.WebApi.Services
{
    internal class InMemoryCacheShapefileCronJobService : CronJobService
    {
        private readonly IMemoryCache _cache;
        private readonly CronJobServiceHealthCheck _cronJobServiceHealthCheck;
        private readonly IOptions<ApplicationOptions> _applicationOptions;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, (string, FileSystemWatcher)> _entries = new ConcurrentDictionary<string, (string, FileSystemWatcher)>();

        public InMemoryCacheShapefileCronJobService(
            IScheduleConfig<InMemoryCacheShapefileCronJobService> scheduleConfig,
            IMemoryCache memoryCache,
            CronJobServiceHealthCheck cronJobServiceHealthCheck,
            IOptions<ApplicationOptions> applicationOptions,
            ILogger<InMemoryCacheShapefileCronJobService> logger)
            : base(scheduleConfig.CronExpression, scheduleConfig.TimeZoneInfo)
        {
            _cache = memoryCache;
            _cronJobServiceHealthCheck = cronJobServiceHealthCheck;
            _applicationOptions = applicationOptions;
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[Start]:{{ServiceName}}", nameof(InMemoryCacheShapefileCronJobService));

            await base.StartAsync(cancellationToken);

            _cronJobServiceHealthCheck.StartupTaskCompleted = true;
        }

        public override Task DoWork(CancellationToken cancellationToken)
        {
            UpdateEntries();

            foreach (var (name, _) in _entries)
            {
                var rootDirectory = _applicationOptions.Value.ShapeConfiguration.ShapeSystemRootDirectory;
                var directory = Enum.Parse<ShapeProperties>(name).GetAttribute<ShapeAttributes>().Directory;
                var file = Enum.Parse<ShapeProperties>(name).GetAttribute<ShapeAttributes>().FileName;

                // ShapeFile Path without File Extension
                var shapeFilePath = Path.Combine(rootDirectory, directory, file);

                var shapefileDataReader = new ShapefileDataReader(shapeFilePath, new GeometryFactory());
                var features = shapefileDataReader.ReadFeatures();

                var memCacheTimeSpan = TimeSpan.FromHours(3);
                _cache.Set(name, features, memCacheTimeSpan);

                _logger.LogInformation($"[CacheRefresh]:{{ServiceName}}|ShapeName:{{ShapeName}}|CacheTimeSpan:{{memCacheTimeSpan}}", nameof(InMemoryCacheShapefileCronJobService), name, memCacheTimeSpan);
            }

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[Stop]:{{ServiceName}}", nameof(InMemoryCacheShapefileCronJobService));
            _cache.Dispose();

            return base.StopAsync(cancellationToken);
        }

        private void UpdateEntries()
        {
            var nameWithShapeAttributes = typeof(ShapeProperties).GetFields()
                .Where(x => x.IsLiteral)
                .Select(x => (x.Name, (ShapeAttributes)x.GetCustomAttributes(typeof(ShapeAttributes), false).Single()));

            foreach (var (name, shapeAttribute) in nameWithShapeAttributes)
            {
                var rootDirectory = _applicationOptions.Value.ShapeConfiguration.ShapeSystemRootDirectory;
                var directory = shapeAttribute.Directory;
                var file = $"{shapeAttribute.FileName}.dbf";

                string shapeDirectoryPath = Path.Combine(rootDirectory, directory);
                string shapeFilePath = Path.Combine(shapeDirectoryPath, file);

                // Watch on *.dbf File
                if (File.Exists(shapeFilePath))
                {
                    var fileSystemWatcher = new FileSystemWatcher
                    {
                        NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                        Filter = file,
                        Path = shapeDirectoryPath,
                        EnableRaisingEvents = true,
                    };
                    fileSystemWatcher.Changed += async (sender, e) =>
                    {
                        _logger.LogInformation($"[FileChanged]:{{ServiceName}}|FileName:{{Name}}", nameof(InMemoryCacheShapefileCronJobService), e.Name);

                        await DoWork(CancellationToken.None);
                    };

                    _entries.TryAdd(name, (shapeFilePath, fileSystemWatcher));
                }
                else
                {
                    _entries.Remove(name, out _);
                    _logger.LogInformation($"[FileNotExists]:{{ServiceName}}|Directory:{{Name}}", nameof(InMemoryCacheShapefileCronJobService), shapeFilePath);
                }
            }
        }
    }
}
