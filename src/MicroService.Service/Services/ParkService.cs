using AutoMapper;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;

namespace MicroService.Service.Services
{
    public class ParkService : AbstractShapeService<ParkShape>, IShapeService<ParkShape>
    {
        public ParkService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<ParkService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.Parks));
        }

        public override ParkShape GetFeatureLookup(double x, double y)
        {
            // Validate Point is in Range
            var point = new Point(x, y);

            var feature = GetFeatures().FirstOrDefault(f => f.Geometry.Contains(point));

            if (feature == null)
            {
                return null;
            }

            return new ParkShape
            {
                ParkName = feature.Attributes["PARK_NAME"]?.ToString(),
                ParkNumber = feature.Attributes["PARKNUM"].ToString(),
                SourceId = long.Parse(feature.Attributes["SOURCE_ID"].ToString()),
                FeatureCode = int.Parse(feature.Attributes["FEAT_CODE"].ToString()),
                SubCode = int.Parse(feature.Attributes["SUB_CODE"].ToString()),
                LandUse = feature.Attributes["LANDUSE"]?.ToString(),
                System = feature.Attributes["SYSTEM"]?.ToString(),
                Status = feature.Attributes["STATUS"]?.ToString(),
                ShapeArea = double.Parse(feature.Attributes["SHAPE_Area"].ToString()),
                ShapeLength = double.Parse(feature.Attributes["SHAPE_Leng"].ToString()),
            };
        }

        public override IEnumerable<ParkShape> GetFeatureLookup(List<KeyValuePair<string, object>> attributes)
        {
            attributes = ValidateFeatureKey(attributes);

            var results = from f in GetFeatures()
                          where attributes.All(pair =>
                          {
                              var value = f.Attributes[pair.Key];
                              var expectedValue = pair.Value;
                              var matchedValue = MatchAttributeValue(value, expectedValue);
                              return matchedValue != null;
                          })
                          select new ParkShape
                          {
                              ParkName = f.Attributes["PARK_NAME"]?.ToString(),
                              ParkNumber = f.Attributes["PARKNUM"].ToString(),
                              SourceId = long.Parse(f.Attributes["SOURCE_ID"].ToString()),
                              FeatureCode = int.Parse(f.Attributes["FEAT_CODE"].ToString()),
                              SubCode = int.Parse(f.Attributes["SUB_CODE"].ToString()),
                              LandUse = f.Attributes["LANDUSE"]?.ToString(),
                              System = f.Attributes["SYSTEM"]?.ToString(),
                              Status = f.Attributes["STATUS"]?.ToString(),
                              ShapeArea = double.Parse(f.Attributes["SHAPE_Area"].ToString()),
                              ShapeLength = double.Parse(f.Attributes["SHAPE_Leng"].ToString()),
                          };

            return results;
        }

        public IEnumerable<ParkShape> GetFeatureList()
        {
            var features = GetFeatures();

            Logger.LogInformation("Feature Count|{count}", features.Count);

            return features.Select(f => new ParkShape
            {
                ParkName = f.Attributes["PARK_NAME"]?.ToString(),
                ParkNumber = f.Attributes["PARKNUM"].ToString(),
                SourceId = long.Parse(f.Attributes["SOURCE_ID"].ToString()),
                FeatureCode = int.Parse(f.Attributes["FEAT_CODE"].ToString()),
                SubCode = int.Parse(f.Attributes["SUB_CODE"].ToString()),
                LandUse = f.Attributes["LANDUSE"]?.ToString(),
                System = f.Attributes["SYSTEM"]?.ToString(),
                Status = f.Attributes["STATUS"]?.ToString(),
                ShapeArea = double.Parse(f.Attributes["SHAPE_Area"].ToString()),
                ShapeLength = double.Parse(f.Attributes["SHAPE_Leng"].ToString()),
            }).Take(20);
        }
    }
}
