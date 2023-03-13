using AutoMapper;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Mappings;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;
using MicroService.Service.Services.Base;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;

namespace MicroService.Service.Services
{
    public class SubwayService : AbstractShapeService<SubwayShape, SubwayShapeProfile>,
        IShapeService<SubwayShape>, IPointService<SubwayShape>
    {
        public SubwayService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<SubwayService> logger)
            : base(logger, mapper)
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

        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            var featureCollection = new FeatureCollection();
            var features = GetFeatureLookup(attributes);

            foreach (var feature in features)
            {
                var featureAttributes = Mapper.Map<IDictionary<string, object>>(feature);
                featureAttributes.Add("ShapeColor", Color.Blue.ToString().ToLower());
                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }

        public List<Point> FindPointsByRadius(Point center, double radius)
        {
            var result = new List<Point>();
            var features = GetFeatures();

            foreach (var feature in features)
            {
                var point = feature.Geometry as Point;
                if (point != null)
                {
                    var distance = center.Distance(point);
                    if (distance <= radius)
                    {
                        result.Add(point);
                    }
                }
            }

            return result;
        }

    }
}
