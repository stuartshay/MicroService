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
    public class NeighborhoodsService<T> : AbstractShapeService<NeighborhoodShape>, IShapeService<NeighborhoodShape>
    {
        public NeighborhoodsService(ShapefileDataReaderResolver shapefileDataReaderResolver)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.Neighborhoods));
        }

        public override NeighborhoodShape GetFeatureLookup(double x, double y)
        {
            // Validate Point is in Range
            var point = new Point(x, y);

            var model = new NeighborhoodShape();
            var features = GetFeatures();
            foreach (var f in features)
            {
                var exists = f.Geometry.Contains(point);
                if (exists)
                {
                    model = new NeighborhoodShape
                    {
                        BoroCode = int.Parse(f.Attributes["BoroCode"].ToString()),
                        BoroName = f.Attributes["BoroName"].ToString(),
                        CountyFIPS = f.Attributes["CountyFIPS"].ToString(),
                        NTACode = f.Attributes["NTACode"].ToString(),
                        NTAName = f.Attributes["NTAName"].ToString(),
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

        public override IEnumerable<NeighborhoodShape> GetFeatureLookup(List<KeyValuePair<string, string>> features)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<NeighborhoodShape> GetFeatureAttributes()
        {
            var features = GetFeatures();
            var results = new List<NeighborhoodShape>(features.Count);

            foreach (var f in features)
            {
                var model = new NeighborhoodShape
                {
                    BoroCode = int.Parse(f.Attributes["BoroCode"].ToString()),
                    BoroName = f.Attributes["BoroName"].ToString(),
                    CountyFIPS = f.Attributes["CountyFIPS"].ToString(),
                    NTACode = f.Attributes["NTACode"].ToString(),
                    NTAName = f.Attributes["NTAName"].ToString(),
                    ShapeArea = double.Parse(f.Attributes["Shape_Area"].ToString()),
                    ShapeLength = double.Parse(f.Attributes["Shape_Leng"].ToString()),
                };

                results.Add(model);
            }

            return results.OrderBy(x => x.BoroCode);
        }

    }
}
