using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using Coordinate = MicroService.Service.Models.Base.Coordinate;

namespace MicroService.Service.Services
{
    public class IndividualLandmarkSiteService : AbstractShapeService<IndividualLandmarkSiteShape>, IShapeService<IndividualLandmarkSiteShape>
    {
        public IndividualLandmarkSiteService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            ILogger<IndividualLandmarkSiteService> logger)
            : base(logger)
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
            attributes = ValidateFeatureKey(attributes);

            var results = GetFeatures()
                .Where(f => attributes.All(pair =>
                {
                    var value = f.Attributes[pair.Key];
                    var expectedValue = pair.Value;
                    var matchedValue = MatchAttributeValue(value, expectedValue);
                    return matchedValue != null;
                }))
                .Select(f => new IndividualLandmarkSiteShape
                {
                    LPNumber = f.Attributes["lpc_lpnumb"].ToString(),
                    AreaName = f.Attributes["lpc_name"].ToString(),
                    BBL = Double.Parse(f.Attributes["bbl"].ToString()),
                    BoroName = f.Attributes["borough"].ToString(),
                    BoroCode = (int)Enum.Parse(typeof(Borough), f.Attributes["borough"].ToString()),
                    ShapeArea = double.Parse(f.Attributes["shape_area"].ToString()),
                    ShapeLength = double.Parse(f.Attributes["shape_leng"].ToString()),
                    Coordinates = new List<Coordinate>(),
                });

            return results;
        }
    }
}
