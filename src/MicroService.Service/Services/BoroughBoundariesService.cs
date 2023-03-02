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
    public class BoroughBoundariesService : AbstractShapeService<BoroughBoundaryShape>, IShapeService<BoroughBoundaryShape>
    {
        public BoroughBoundariesService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<BoroughBoundariesService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.BoroughBoundaries));
            Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }


        public virtual BoroughBoundaryShape GetFeatureLookup(double x, double y)
        {
            var point = new Point(x, y);

            var features = GetFeatures();

            var feature = features.FirstOrDefault(f => f.Geometry.Contains(point));

            if (feature == null)
            {
                return null;
            }

            return new BoroughBoundaryShape
            {
                BoroCode = int.Parse(feature.Attributes["BoroCode"].ToString()),
                BoroName = feature.Attributes["BoroName"].ToString(),
                ShapeArea = double.Parse(feature.Attributes["Shape_Area"].ToString()),
                ShapeLength = double.Parse(feature.Attributes["Shape_Leng"].ToString()),
            };
        }

        public override IEnumerable<BoroughBoundaryShape> GetFeatureLookup(List<KeyValuePair<string, object>> attributes)
        {
            // var shape = Mapper.Map<BoroughBoundaryShape>(attributes);
            attributes = ValidateFeatureKey(attributes);

            var results = from f in GetFeatures()
                          where attributes.All(pair =>
                                    {
                                        var value = f.Attributes[pair.Key];
                                        var expectedValue = pair.Value;
                                        var matchedValue = MatchAttributeValue(value, expectedValue);
                                        return matchedValue != null;
                                    })
                          select new BoroughBoundaryShape
                          {
                              BoroCode = int.Parse(f.Attributes["BoroCode"].ToString()),
                              BoroName = f.Attributes["BoroName"].ToString(),
                              ShapeArea = double.Parse(f.Attributes["Shape_Area"].ToString()),
                              ShapeLength = double.Parse(f.Attributes["Shape_Leng"].ToString()),
                              //BoundingBox = f.BoundingBox,
                          };

            return results;
        }

        public override IEnumerable<Geometry> GetGeometryLookup(List<KeyValuePair<string, object>> attributes)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<BoroughBoundaryShape> GetFeatureList()
        {
            var features = GetFeatures();
            Logger.LogInformation("FeatureCount {FeatureCount}", features.Count);

            var results = features.Select(f => new BoroughBoundaryShape
            {
                BoroCode = int.Parse(f.Attributes["BoroCode"].ToString()),
                BoroName = f.Attributes["BoroName"].ToString(),
                ShapeArea = double.Parse(f.Attributes["Shape_Area"].ToString()),
                ShapeLength = double.Parse(f.Attributes["Shape_Leng"].ToString()),
            }).OrderBy(x => x.BoroCode);

            return results;
        }

    }
}
