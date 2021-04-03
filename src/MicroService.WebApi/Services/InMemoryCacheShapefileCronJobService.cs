using System;
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
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace MicroService.WebApi.Services
{
    internal class InMemoryCacheShapefileCronJobService : CronJobService
    {
        private readonly IMemoryCache _cache;
        private readonly IOptions<ApplicationOptions> _applicationOptions;
        private readonly ILogger _logger;

        public InMemoryCacheShapefileCronJobService(IScheduleConfig<InMemoryCacheShapefileCronJobService> scheduleConfig, IMemoryCache memoryCache, IOptions<ApplicationOptions> applicationOptions, ILogger<InMemoryCacheShapefileCronJobService> logger)
            : base(scheduleConfig.CronExpression, scheduleConfig.TimeZoneInfo)
        {
            _cache = memoryCache;
            _applicationOptions = applicationOptions;
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[Start] {nameof(InMemoryCacheShapefileCronJobService)}");

            return base.StartAsync(cancellationToken);
        }

        public override Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} {nameof(InMemoryCacheShapefileCronJobService)} is working.");

            var nameWithShapeAttributes = typeof(ShapeProperties).GetFields()
                .Where(x => x.IsLiteral)
                .Select(x => (x.Name, (ShapeAttributes)x.GetCustomAttributes(typeof(ShapeAttributes), false).Single()));
            foreach (var (name, shapeAttribute) in nameWithShapeAttributes)
            {
                var shapeDirectory = $"{Path.Combine(_applicationOptions.Value.ShapeConfiguration.ShapeRootDirectory, shapeAttribute.Directory, shapeAttribute.FileName)}";
                string shapePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), shapeDirectory));

                var shapefileDataReader = new ShapefileDataReader(shapePath, new GeometryFactory());

                var features = shapefileDataReader.ReadFeatures();

                _cache.Set(name, features, TimeSpan.FromHours(3));
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
