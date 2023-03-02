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
    public class NypdPolicePrecinctService : AbstractShapeService<NypdPrecinctShape>, IShapeService<NypdPrecinctShape>
    {
        public NypdPolicePrecinctService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<NypdPolicePrecinctService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.NypdPolicePrecincts));
        }

        public override NypdPrecinctShape GetFeatureLookup(double x, double y)
        {
            var point = new Point(x, y);

            var feature = GetFeatures().FirstOrDefault(f => f.Geometry.Contains(point));
            if (feature == null)
            {
                return null;
            }

            return new NypdPrecinctShape
            {
                Precinct = int.Parse(feature.Attributes["Precinct"].ToString()),
                ShapeArea = double.Parse(feature.Attributes["Shape_Area"].ToString()),
                ShapeLength = double.Parse(feature.Attributes["Shape_Leng"].ToString()),
            };
        }

        public override IEnumerable<NypdPrecinctShape> GetFeatureLookup(List<KeyValuePair<string, object>> attributes)
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
                          select new NypdPrecinctShape
                          {
                              Precinct = int.Parse(f.Attributes["Precinct"].ToString()),
                              ShapeArea = double.Parse(f.Attributes["Shape_Area"].ToString()),
                              ShapeLength = double.Parse(f.Attributes["Shape_Leng"].ToString()),
                          };

            return results;
        }

        public IEnumerable<NypdPrecinctShape> GetFeatureList()
        {
            var features = GetFeatures();

            return features.Select(f => new NypdPrecinctShape
            {
                Precinct = int.Parse(f.Attributes["Precinct"].ToString()),
                ShapeArea = double.Parse(f.Attributes["Shape_Area"].ToString()),
                ShapeLength = double.Parse(f.Attributes["Shape_Leng"].ToString()),
            });
        }

    }
}
