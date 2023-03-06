using AutoMapper;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;

namespace MicroService.Service.Services
{
    public class SubwayService : AbstractShapeService<SubwayShape>, IShapeService<SubwayShape>, IPointService<SubwayShape>
    {
        public SubwayService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<SubwayService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.Subway));
        }

        public virtual SubwayShape GetFeatureLookup(double x, double y)
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

        public IEnumerable<SubwayShape> GetFeatureLookup(List<KeyValuePair<string, object>> attributes)
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
                          select new SubwayShape
                          {
                              Line = f.Attributes["line"].ToString(),
                              Name = f.Attributes["name"].ToString(),
                              ObjectId = int.Parse(f.Attributes["objectid"].ToString()),
                              //ShapeArea = double.Parse(f.Attributes["Shape_Area"].ToString()),
                              //ShapeLength = double.Parse(f.Attributes["Shape_Leng"].ToString()),
                              //Coordinates = new List<Coordinate>(),
                          };

            return results;
        }

        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<SubwayShape> GetFeatureList()
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
