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
    public class NypdSectorsService : AbstractShapeService<NypdSectorShape>, IShapeService<NypdSectorShape>
    {
        public NypdSectorsService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<NypdSectorsService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.NypdSectors));
        }

        public virtual NypdSectorShape GetFeatureLookup(double x, double y)
        {
            var point = new Point(x, y);

            var feature = GetFeatures().FirstOrDefault(f => f.Geometry.Contains(point));
            if (feature == null)
            {
                return null;
            }

            return new NypdSectorShape
            {
                Pct = feature.Attributes["pct"]?.ToString(),
                Sector = feature.Attributes["sector"]?.ToString(),
                PatrolBoro = feature.Attributes["patrol_bor"]?.ToString(),
                Phase = feature.Attributes["phase"]?.ToString(),
            };
        }

        public IEnumerable<NypdSectorShape> GetFeatureLookup(List<KeyValuePair<string, object>> attributes)
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
                          select new NypdSectorShape
                          {
                              Pct = f.Attributes["pct"].ToString(),
                              Sector = f.Attributes["sector"].ToString(),
                              PatrolBoro = f.Attributes["patrol_bor"].ToString(),
                              Phase = f.Attributes["phase"].ToString(),
                          };

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
                .Select(f => new NypdSectorShape
                {
                    Pct = f.Attributes["pct"].ToString(),
                    Sector = f.Attributes["sector"].ToString(),
                    PatrolBoro = f.Attributes["patrol_bor"].ToString(),
                    Phase = f.Attributes["phase"].ToString(),
                    Geometry = f.Geometry,
                });

            foreach (var feature in features)
            {
                var featureProperties = EnumHelper.GetPropertiesWithoutExcludedAttribute<NypdSectorShape, FeatureCollectionExcludeAttribute>();
                var featureAttributes = featureProperties
                    .ToDictionary(prop => prop.Name, prop => prop.GetValue(feature, null));

                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }

        public IEnumerable<NypdSectorShape> GetFeatureList()
        {
            var features = GetFeatures();

            return features.Select(f => new NypdSectorShape
            {
                Pct = f.Attributes["pct"].ToString(),
                Sector = f.Attributes["sector"].ToString(),
                PatrolBoro = f.Attributes["patrol_bor"].ToString(),
                Phase = f.Attributes["phase"].ToString(),
            });
        }
    }
}
