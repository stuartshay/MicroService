using MicroService.Service.Models;
using Microsoft.Extensions.Caching.Memory;
using NetTopologySuite.Features;
using System;
using System.Collections.Generic;

namespace MicroService.Service.Helpers
{
    public interface IShapefileDataReaderService
    {
        ShapefileProperties ShapeHeader { get; }

        ShapeDatabaseProperties DbaseHeader { get; }

        IReadOnlyCollection<Feature> GetFeatures();
    }

    public sealed class CachedShapefileDataReader : IShapefileDataReaderService
    {
        private readonly IMemoryCache _cache;
        private readonly string _shapeProperties;
        private readonly string _fileName;

        public ShapefileProperties ShapeHeader { get; }

        public ShapeDatabaseProperties DbaseHeader { get; }

        public CachedShapefileDataReader(IMemoryCache memoryCache, string shapeProperties, string fileName)
        {
            _cache = memoryCache;
            _shapeProperties = shapeProperties;
            _fileName = fileName;
            (ShapeHeader, DbaseHeader) = ShapefileSource.ReadMetadata(fileName);
        }

        public IReadOnlyCollection<Feature> GetFeatures() =>
            _cache.GetOrCreate(_shapeProperties, cacheEntry =>
            {
                var features = ShapefileSource.ReadFeatures(_fileName);

                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(3);

                return features;
            }) ?? [];
    }
}
