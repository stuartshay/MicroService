using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;
using Coordinate = MicroService.Service.Models.Base.Coordinate;

namespace MicroService.Service.Services
{
    public class BoroughBoundariesService : AbstractShapeService<BoroughBoundaryShape>, IShapeService<BoroughBoundaryShape>
    {
        public BoroughBoundariesService(ShapefileDataReaderResolver shapefileDataReaderResolver)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.BoroughBoundaries));
        }

        public override BoroughBoundaryShape GetFeatureLookup(double x, double y)
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
                Coordinates = new List<Coordinate>()
            };
        }

        public override IEnumerable<BoroughBoundaryShape> GetFeatureLookup(List<KeyValuePair<string, object>> attributes)
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
                          select new BoroughBoundaryShape
                          {
                              BoroCode = int.Parse(f.Attributes["BoroCode"].ToString()),
                              BoroName = f.Attributes["BoroName"].ToString(),
                              ShapeArea = double.Parse(f.Attributes["Shape_Area"].ToString()),
                              ShapeLength = double.Parse(f.Attributes["Shape_Leng"].ToString()),
                              Coordinates = new List<Coordinate>(),
                          };

            return results;
        }

        public IEnumerable<BoroughBoundaryShape> GetFeatureAttributes()
        {
            var features = GetFeatures();

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
