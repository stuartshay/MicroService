using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;
using Coordinate = MicroService.Service.Models.Base.Coordinate;

namespace MicroService.Service.Services
{
    public class NypdSectorsService : AbstractShapeService<NypdSectorShape>, IShapeService<NypdSectorShape>
    {
        public NypdSectorsService(ShapefileDataReaderResolver shapefileDataReaderResolver)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.NypdSectors));
        }

        public override NypdSectorShape GetFeatureLookup(double x, double y)
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
                Coordinates = new List<Coordinate>(),
            };
        }

        public override IEnumerable<NypdSectorShape> GetFeatureLookup(List<KeyValuePair<string, string>> attributes)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<NypdSectorShape> GetFeatureAttributes()
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
