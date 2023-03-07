using AutoMapper;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Mappings;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;

namespace MicroService.Service.Services
{
    public class NychaDevelopmentService : AbstractShapeService<NychaDevelopmentShape, FeatureToNychaDevelopmentShapeProfile>, IShapeService<NychaDevelopmentShape>
    {
        public NychaDevelopmentService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<NychaDevelopmentService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.NychaDevelopments));
        }

        public virtual NychaDevelopmentShape GetFeatureLookup(double x, double y)
        {
            var point = new Point(x, y);

            var features = GetFeatures();
            var feature = features.FirstOrDefault(f => f.Geometry.Contains(point));

            if (feature == null)
            {
                return null;
            }

            return Mapper.Map<NychaDevelopmentShape>(feature);
        }

        public IEnumerable<NychaDevelopmentShape> GetFeatureLookup(List<KeyValuePair<string, object>> attributes)
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
                          select new NychaDevelopmentShape
                          {
                              Development = f.Attributes["DEVELOPMEN"].ToString(),
                              TdsNumber = f.Attributes["TDS_NUM"]?.ToString(),
                              Borough = f.Attributes["BOROUGH"]?.ToString(),
                              ShapeArea = 0,
                              ShapeLength = 0,
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
                .Select(f => new NychaDevelopmentShape
                {
                    Development = f.Attributes["DEVELOPMEN"].ToString(),
                    TdsNumber = f.Attributes["TDS_NUM"]?.ToString(),
                    Borough = f.Attributes["BOROUGH"]?.ToString(),
                    ShapeArea = 0,
                    ShapeLength = 0,
                    Geometry = f.Geometry,
                });

            foreach (var feature in features)
            {
                var featureProperties = EnumHelper.GetPropertiesWithoutExcludedAttribute<NychaDevelopmentShape, FeatureCollectionExcludeAttribute>();
                var featureAttributes = featureProperties
                    .ToDictionary(prop => prop.Name, prop => prop.GetValue(feature, null));

                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }

        public IEnumerable<NychaDevelopmentShape> GetFeatureList()
        {
            var features = GetFeatures();

            var results = Mapper.Map<IEnumerable<NychaDevelopmentShape>>(features);
            return results;
        }

    }
}
