using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MicroService.Service.Configuration;
using MicroService.Service.Extensions;
using MicroService.Service.Models.Enum;
using MicroService.WebApi.Services.Cron;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace MicroService.WebApi.Services
{
    internal class InMemoryCacheShapefileCronJobService : CronJobService
    {
        private readonly IMemoryCache _cache;
        private readonly IOptions<ApplicationOptions> _applicationOptions;
        private readonly ILogger _logger;
        private List<(string, string, FileSystemWatcher)> _entries;

        public InMemoryCacheShapefileCronJobService(
            IScheduleConfig<InMemoryCacheShapefileCronJobService> scheduleConfig,
            IMemoryCache memoryCache,
            IOptions<ApplicationOptions> applicationOptions,
            ILogger<InMemoryCacheShapefileCronJobService> logger)
            : base(scheduleConfig.CronExpression, scheduleConfig.TimeZoneInfo)
        {
            _cache = memoryCache;
            _applicationOptions = applicationOptions;
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            //_logger.LogInformation($"[Start]{ServiceName}", nameof(InMemoryCacheShapefileCronJobService));
           // _logger.LogInformation($"Cache Refresh:{{ServiceName}}|Name:{{Name}}|CacheTimeSpan:{memCacheTimeSpan}}}", nameof(InMemoryCacheShapefileCronJobService), name, memCacheTimeSpan);

            _logger.LogInformation($"[Start]:{{ServiceName}}", nameof(InMemoryCacheShapefileCronJobService));



            _entries = new List<(string, string, FileSystemWatcher)>();
            var nameWithShapeAttributes = typeof(ShapeProperties).GetFields()
                .Where(x => x.IsLiteral)
                .Select(x => (x.Name, (ShapeAttributes)x.GetCustomAttributes(typeof(ShapeAttributes), false).Single()));
            foreach (var (name, shapeAttribute) in nameWithShapeAttributes)
            {
                var shapeDirectory = $"{Path.Combine(_applicationOptions.Value.ShapeConfiguration.ShapeRootDirectory, shapeAttribute.Directory, shapeAttribute.FileName)}";
                string shapePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), shapeDirectory));

                var fileSystemWatcher = new FileSystemWatcher()
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                    Filter = $"{shapeAttribute.FileName}.dbf",
                    Path = Path.GetDirectoryName(shapePath),
                    EnableRaisingEvents = true,
                };
                fileSystemWatcher.Changed += async (sender, e) =>
                {
                    _logger.LogInformation($"File {e.Name} was changed");

                    await DoWork(CancellationToken.None);
                };

                _entries.Add((name, shapePath, fileSystemWatcher));
            }

            return base.StartAsync(cancellationToken);
        }

        public override Task DoWork(CancellationToken cancellationToken)
        {
            foreach (var (name, shapePath, _) in _entries)
            {
                var shapefileDataReader = new ShapefileDataReader(shapePath, new GeometryFactory());

                var features = shapefileDataReader.ReadFeatures();

                var memCacheTimeSpan = TimeSpan.FromHours(3);
                _cache.Set(name, features, memCacheTimeSpan);

                _logger.LogInformation($"[CacheRefresh]:{{ServiceName}}|Name:{{Name}}|CacheTimeSpan:{memCacheTimeSpan}}}", nameof(InMemoryCacheShapefileCronJobService), name, memCacheTimeSpan);
            }

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[Stop] {nameof(InMemoryCacheShapefileCronJobService)}");

            _cache.Dispose();

            return base.StopAsync(cancellationToken);
        }
    }
}
