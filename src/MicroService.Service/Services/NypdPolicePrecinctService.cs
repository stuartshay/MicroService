using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;
using Coordinate = MicroService.Service.Models.Base.Coordinate;

namespace MicroService.Service.Services
{
    public class NypdPolicePrecinctService : AbstractShapeService<NypdPrecinctShape>, IShapeService<NypdPrecinctShape>
    {
        public NypdPolicePrecinctService(ShapefileDataReaderResolver shapefileDataReaderResolver)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.NypdPolicePrecincts));
        }

        public override NypdPrecinctShape GetFeatureLookup(double x, double y)
        {
            // Validate Point is in Range
            var point = new Point(x, y);

            var model = new NypdPrecinctShape();

            var features = GetFeatures();
            foreach (var f in features)
            {
                var exists = f.Geometry.Contains(point);
                if (exists)
                {
                    model = new NypdPrecinctShape
                    {
                        Precinct = f.Attributes["Precinct"].ToString(),
                        ShapeArea = double.Parse(f.Attributes["Shape_Area"].ToString()),
                        ShapeLength = double.Parse(f.Attributes["Shape_Leng"].ToString()),
                        Coordinates = new List<Coordinate>(),
                    };
                }

            }

            if (!model.ArePropertiesNotNull())
            {
                return null;
            }

            return model;
        }

        public override IEnumerable<NypdPrecinctShape> GetFeatureLookup(List<KeyValuePair<string, string>> attributes)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<NypdPrecinctShape> GetFeatureAttributes()
        {
            var features = GetFeatures();

            return features.Select(f => new NypdPrecinctShape
            {
                Precinct = f.Attributes["Precinct"].ToString(),
            });
        }

    }
}
