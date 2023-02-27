using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;
using Coordinate = MicroService.Service.Models.Base.Coordinate;

namespace MicroService.Service.Services
{
    public class ParkService : AbstractShapeService<ParkShape>, IShapeService<ParkShape>
    {
        public ParkService(ShapefileDataReaderResolver shapefileDataReaderResolver)
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
                ParkName = feature.Attributes["PARK_NAME"].ToString(),
                ParkNumber = feature.Attributes["PARKNUM"].ToString(),
                SourceId = long.Parse(feature.Attributes["SOURCE_ID"].ToString()),
                FeatureCode = int.Parse(feature.Attributes["FEAT_CODE"].ToString()),
                SubCode = int.Parse(feature.Attributes["SUB_CODE"].ToString()),
                LandUse = feature.Attributes["LANDUSE"] != null ? feature.Attributes["LANDUSE"].ToString() : string.Empty,
                ShapeArea = double.Parse(feature.Attributes["SHAPE_Area"].ToString()),
                ShapeLength = double.Parse(feature.Attributes["SHAPE_Leng"].ToString()),
                Coordinates = new List<Coordinate>(),
            };
        }

        public override IEnumerable<ParkShape> GetFeatureLookup(List<KeyValuePair<string, object>> attributes)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<ParkShape> GetFeatureAttributes()
        {
            var features = GetFeatures();

            return features.Select(f => new ParkShape
            {
                ParkName = f.Attributes["PARK_NAME"].ToString(),
                ParkNumber = f.Attributes["PARKNUM"].ToString(),
                SourceId = long.Parse(f.Attributes["SOURCE_ID"].ToString()),
                FeatureCode = int.Parse(f.Attributes["FEAT_CODE"].ToString()),
                SubCode = int.Parse(f.Attributes["SUB_CODE"].ToString()),
                LandUse = f.Attributes["LANDUSE"].ToString(),
                ShapeArea = double.Parse(f.Attributes["SHAPE_Area"].ToString()),
                ShapeLength = double.Parse(f.Attributes["SHAPE_Leng"].ToString()),
            });
        }
    }
}
