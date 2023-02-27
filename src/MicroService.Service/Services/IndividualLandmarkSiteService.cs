using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using Coordinate = MicroService.Service.Models.Base.Coordinate;

namespace MicroService.Service.Services
{
    public class IndividualLandmarkSiteService : AbstractShapeService<IndividualLandmarkSiteShape>, IShapeService<IndividualLandmarkSiteShape>
    {
        public IndividualLandmarkSiteService(ShapefileDataReaderResolver shapefileDataReaderResolver)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.IndividualLandmarkSite));
        }

        public IEnumerable<IndividualLandmarkSiteShape> GetFeatureAttributes()
        {
            var features = GetFeatures();

            return features.Select(f => new IndividualLandmarkSiteShape
            {
                LPNumber = f.Attributes["lpc_lpnumb"].ToString(),
                AreaName = f.Attributes["lpc_name"].ToString(),
                BoroName = f.Attributes["borough"].ToString(),
                BoroCode = (int)Enum.Parse(typeof(Borough), f.Attributes["borough"].ToString()),
                ShapeArea = double.Parse(f.Attributes["shape_area"].ToString()),
                ShapeLength = double.Parse(f.Attributes["shape_leng"].ToString()),
            });
        }

        public override IndividualLandmarkSiteShape GetFeatureLookup(double x, double y)
        {
            // Convert Nad83 to Wgs 
            var result = GeoTransformationHelper.ConvertNad83ToWgs84(x, y);
            var wgs84Point = new { X = result.Item1, Y = result.Item2 };

            // Validate Point is in Range
            var point = new Point(wgs84Point.X.Value, wgs84Point.Y.Value);

            var model = new IndividualLandmarkSiteShape();

            var features = GetFeatures();
            foreach (var f in features)
            {
                var exists = f.Geometry.Contains(point);
                if (exists)
                {
                    var borough = f.Attributes["borough"].ToString();
                    model = new IndividualLandmarkSiteShape
                    {
                        LPNumber = f.Attributes["lpc_lpnumb"].ToString(),
                        AreaName = f.Attributes["lpc_name"].ToString(),
                        BoroName = borough,
                        BoroCode = (int)Enum.Parse(typeof(Borough), borough),
                        ShapeArea = double.Parse(f.Attributes["shape_area"].ToString()),
                        ShapeLength = double.Parse(f.Attributes["shape_leng"].ToString()),
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

        public override IEnumerable<IndividualLandmarkSiteShape> GetFeatureLookup(List<KeyValuePair<string, object>> attributes)
        {
            // Get Shape Feature Names
            for (int i = 0; i < attributes.Count; i++)
            {
                var key = attributes[i].Key;
                var featureName = GetFeatureName(key);
                attributes[i] = new KeyValuePair<string, object>(featureName, attributes[i].Value);
            }

            var results = from f in GetFeatures()
                          where attributes.All(pair => f.Attributes[pair.Key] as string == pair.Value.ToString())
                          select new IndividualLandmarkSiteShape
                          {
                              LPNumber = f.Attributes["lpc_lpnumb"].ToString(),
                              AreaName = f.Attributes["lpc_name"].ToString(),
                              BoroName = f.Attributes["borough"].ToString(),
                              BoroCode = (int)Enum.Parse(typeof(Borough), f.Attributes["borough"].ToString()),
                              ShapeArea = double.Parse(f.Attributes["shape_area"].ToString()),
                              ShapeLength = double.Parse(f.Attributes["shape_leng"].ToString()),
                              Coordinates = new List<Coordinate>(),
                          };

            return results;
        }
    }
}
