using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;

namespace MicroService.Service.Services
{
    public class SubwayService : AbstractShapeService<SubwayShape>, IShapeService<SubwayShape>
    {
        public SubwayService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            ILogger<SubwayService> logger)
            : base(logger)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.Subway));
        }

        public override SubwayShape GetFeatureLookup(double x, double y)
        {
            // Validate Point is in Range
            var result = GeoTransformationHelper.ConvertNad83ToWgs84(x, y);
            var point = new Point(result.Item1.Value, result.Item2.Value);

            var features = GetFeatures();
            var subwayStops = new List<SubwayShape>(features.Count);

            foreach (var f in features)
            {
                var distance = f.Geometry.Distance(point);
                var model = new SubwayShape
                {
                    Line = f.Attributes["line"].ToString(),
                    Name = f.Attributes["name"].ToString(),
                    ObjectId = int.Parse(f.Attributes["objectid"].ToString()),
                    Distance = distance,
                };

                subwayStops.Add(model);
            }

            var orderedStops = subwayStops.OrderBy(x => x.Distance);
            var nearest = orderedStops.FirstOrDefault();

            return nearest;
        }

        public override IEnumerable<SubwayShape> GetFeatureLookup(List<KeyValuePair<string, object>> attributes)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<SubwayShape> GetFeatureAttributes()
        {
            var features = GetFeatures();

            return features.Select(f => new SubwayShape
            {
                Line = f.Attributes["line"].ToString(),
                Name = f.Attributes["name"].ToString(),
                ObjectId = int.Parse(f.Attributes["objectid"].ToString()),
            });
        }
    }
}
