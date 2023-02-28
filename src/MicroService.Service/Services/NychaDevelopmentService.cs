using AutoMapper;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;
using Coordinate = MicroService.Service.Models.Base.Coordinate;

namespace MicroService.Service.Services
{
    public class NychaDevelopmentService : AbstractShapeService<NychaDevelopmentShape>, IShapeService<NychaDevelopmentShape>
    {
        public NychaDevelopmentService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<NychaDevelopmentService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.NychaDevelopments));
        }

        public override NychaDevelopmentShape GetFeatureLookup(double x, double y)
        {
            var point = new Point(x, y);

            var features = GetFeatures();
            var feature = features.FirstOrDefault(f => f.Geometry.Contains(point));

            if (feature == null)
            {
                return null;
            }

            return new NychaDevelopmentShape
            {
                Development = feature.Attributes["DEVELOPMEN"].ToString(),
                TdsNumber = feature.Attributes["TDS_NUM"]?.ToString(),
                Borough = feature.Attributes["BOROUGH"]?.ToString(),
            };
        }

        public override IEnumerable<NychaDevelopmentShape> GetFeatureLookup(List<KeyValuePair<string, object>> attributes)
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
                              Coordinates = new List<Coordinate>(),
                          };

            return results;
        }

        public IEnumerable<NychaDevelopmentShape> GetFeatureAttributes()
        {
            var features = GetFeatures();

            return features.Select(f => new NychaDevelopmentShape
            {
                Development = f.Attributes["DEVELOPMEN"].ToString(),
                TdsNumber = f.Attributes["TDS_NUM"]?.ToString(),
                Borough = f.Attributes["BOROUGH"]?.ToString(),
            });
        }
    }
}
