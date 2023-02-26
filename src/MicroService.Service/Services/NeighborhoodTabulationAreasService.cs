using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;
using Coordinate = MicroService.Service.Models.Base.Coordinate;

namespace MicroService.Service.Services
{
    public class NeighborhoodTabulationAreasService : AbstractShapeService<NeighborhoodTabulationAreaShape>, IShapeService<NeighborhoodTabulationAreaShape>
    {
        public NeighborhoodTabulationAreasService(ShapefileDataReaderResolver shapefileDataReaderResolver)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.NeighborhoodTabulationAreas));
        }

        public override NeighborhoodTabulationAreaShape GetFeatureLookup(double x, double y)
        {
            // Validate Point is in Range
            var point = new Point(x, y);

            var features = GetFeatures();
            var f = features.FirstOrDefault(f => f.Geometry.Contains(point));

            if (f == null)
            {
                return null;
            }

            return new NeighborhoodTabulationAreaShape
            {
                BoroCode = int.Parse(f.Attributes["BoroCode"].ToString()),
                BoroName = f.Attributes["BoroName"].ToString(),
                CountyFIPS = f.Attributes["CountyFIPS"].ToString(),
                NTA2020 = f.Attributes["NTA2020"].ToString(),
                NTAName = f.Attributes["BoroCode"].ToString(),
                NTAAbbrev = f.Attributes["NTAName"].ToString(),
                NTAType = int.Parse(f.Attributes["NTAType"].ToString()),
                CDTA2020 = f.Attributes["CDTA2020"].ToString(),
                CDTAName = f.Attributes["CDTAName"].ToString(),
                ShapeArea = double.Parse(f.Attributes["Shape_Area"].ToString()),
                ShapeLength = double.Parse(f.Attributes["Shape_Leng"].ToString()),
                Coordinates = new List<Coordinate>(),
            };

        }

        public override IEnumerable<NeighborhoodTabulationAreaShape> GetFeatureLookup(
            List<KeyValuePair<string, object>> attributes)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<NeighborhoodTabulationAreaShape> GetFeatureAttributes()
        {
            var features = GetFeatures();

            return features.Select(f => new NeighborhoodTabulationAreaShape
            {
                BoroCode = int.Parse(f.Attributes["BoroCode"].ToString()),
                BoroName = f.Attributes["BoroName"].ToString(),
                CountyFIPS = f.Attributes["CountyFIPS"].ToString(),
                NTA2020 = f.Attributes["NTA2020"].ToString(),
                NTAName = f.Attributes["BoroCode"].ToString(),
                NTAAbbrev = f.Attributes["NTAName"].ToString(),
                NTAType = int.Parse(f.Attributes["NTAType"].ToString()),
                CDTA2020 = f.Attributes["CDTA2020"].ToString(),
                CDTAName = f.Attributes["CDTAName"].ToString(),
                ShapeArea = double.Parse(f.Attributes["Shape_Area"].ToString()),
                ShapeLength = double.Parse(f.Attributes["Shape_Leng"].ToString()),
            });
        }
    }
}
