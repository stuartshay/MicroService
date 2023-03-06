using AutoMapper;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
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

        public virtual ParkShape GetFeatureLookup(double x, double y)
        {
            var point = new Point(x, y);

            var feature = GetFeatures().FirstOrDefault(f => f.Geometry.Contains(point));

            if (feature == null)
            {
                return null;
            }

            return Mapper.Map<ParkShape>(feature);
        }

        public IEnumerable<ParkShape> GetFeatureLookup(List<KeyValuePair<string, object>> attributes)
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
                          select Mapper.Map<ParkShape>(f);

            return results;
        }

        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            attributes = ValidateFeatureKey(attributes);
            var featureCollection = new FeatureCollection();

            var features = GetFeatures()
                .Where(f => attributes.All(pair =>
                {
                    var value = f.Attributes[pair.Key];
                    var expectedValue = pair.Value;
                    var matchedValue = MatchAttributeValue(value, expectedValue);
                    return matchedValue != null;
                }))
                .Select(f => new ParkShape
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
                    Geometry = f.Geometry,
                });

            foreach (var feature in features)
            {
                var featureProperties = EnumHelper.GetPropertiesWithoutExcludedAttribute<ParkShape, FeatureCollectionExcludeAttribute>();
                var featureAttributes = featureProperties
                    .ToDictionary(prop => prop.Name, prop => prop.GetValue(feature, null));

                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }

        public IEnumerable<ParkShape> GetFeatureList()
        {
            var features = GetFeatures();

            Logger.LogInformation("Feature Count|{count}", features.Count);

            // HARD CODE TAKE 100
            return Mapper.Map<IEnumerable<ParkShape>>(features).Take(100);
        }
    }
}
