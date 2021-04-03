using MicroService.Service.Extensions;
using Microsoft.Extensions.Caching.Memory;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;

namespace MicroService.Service.Helpers
{
    public interface IShapefileDataReaderService
    {
        ShapefileHeader ShapeHeader { get; }

        DbaseFileHeader DbaseHeader { get; }

        IReadOnlyCollection<Feature> GetFeatures();
    }

    public sealed class CachedShapefileDataReader : ShapefileDataReader, IShapefileDataReaderService
    {
        private readonly IMemoryCache _cache;
        private readonly string _shapeProperties;

        public CachedShapefileDataReader(IMemoryCache memoryCache, string shapeProperties, string fileName) : base(fileName, new GeometryFactory())
        {
            _cache = memoryCache;
            _shapeProperties = shapeProperties;
        }

        public IReadOnlyCollection<Feature> GetFeatures() =>
            _cache.GetOrCreate(_shapeProperties, cacheEntry =>
            {
                var features = this.ReadFeatures();

                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(3);

                return features;
            });
    }
}
