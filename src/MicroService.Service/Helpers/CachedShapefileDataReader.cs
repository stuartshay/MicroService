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

        public CachedShapefileDataReader(IMemoryCache memoryCache, string fileName) : base(fileName, new GeometryFactory())
        {
            _cache = memoryCache;
        }

        public IReadOnlyCollection<Feature> GetFeatures() =>
            _cache.GetOrCreate("Features", cacheEntry =>
            {
                List<Feature> features = new List<Feature>();
                while (Read())
                {
                    Feature feature = new Feature();
                    AttributesTable attributesTable = new AttributesTable();
                    DbaseFileHeader header = DbaseHeader;

                    string[] keys = new string[header.NumFields];
                    var geometry = Geometry;

                    for (int i = 0; i < header.NumFields; i++)
                    {
                        DbaseFieldDescriptor fldDescriptor = header.Fields[i];
                        keys[i] = fldDescriptor.Name;

                        // First Field Geometry
                        var value = GetValue(i + 1);
                        attributesTable.Add(fldDescriptor.Name, value);
                    }

                    feature.Geometry = geometry;
                    feature.Attributes = attributesTable;
                    features.Add(feature);
                }

                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(3);

                return features;
            });
    }
}
