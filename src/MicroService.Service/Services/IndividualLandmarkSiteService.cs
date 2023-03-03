using AutoMapper;
using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroService.Service.Services
{
    public class IndividualLandmarkSiteService : AbstractShapeService<IndividualLandmarkSiteShape>, IShapeService<IndividualLandmarkSiteShape>
    {
        public IndividualLandmarkSiteService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<IndividualLandmarkSiteService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.IndividualLandmarkSite));
        }

        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IndividualLandmarkSiteShape> GetFeatureList()
        {
            var features = GetFeatures();

            return features.Select(f => new IndividualLandmarkSiteShape
            {
                LPNumber = f.Attributes["lpc_lpnumb"].ToString(),
                AreaName = f.Attributes["lpc_name"].ToString(),

                BoroCode = EnumHelper.IsEnumValid<Borough>(f.Attributes["borough"].ToString()) && f.Attributes["borough"] != null ?
                             (int)Enum.Parse(typeof(Borough), f.Attributes["borough"].ToString()) : 0,

                BoroName = EnumHelper.IsEnumValid<Borough>(f.Attributes["borough"].ToString()) && f.Attributes["borough"] != null ?
                           f.Attributes["borough"].ToString() : null,

                BBL = f.Attributes["bbl"] != null ? Double.Parse(f.Attributes["bbl"].ToString()) : 0,
                ShapeArea = double.Parse(f.Attributes["shape_area"].ToString()),
                ShapeLength = double.Parse(f.Attributes["shape_leng"].ToString()),
            });
        }

        public virtual IndividualLandmarkSiteShape GetFeatureLookup(double x, double y)
        {
            // Convert Nad83 to Wgs 
            var result = GeoTransformationHelper.ConvertNad83ToWgs84(x, y);
            var wgs84Point = new { X = result.Item1, Y = result.Item2 };

            var point = new Point(wgs84Point.X.Value, wgs84Point.Y.Value);

            var features = GetFeatures();

            var feature = features.FirstOrDefault(f => f.Geometry.Contains(point));

            if (feature == null)
            {
                return null;
            }

            return new IndividualLandmarkSiteShape
            {
                LPNumber = feature.Attributes["lpc_lpnumb"].ToString(),
                AreaName = feature.Attributes["lpc_name"].ToString(),
                BoroCode = EnumHelper.IsEnumValid<Borough>(feature.Attributes["borough"].ToString()) && feature.Attributes["borough"] != null ?
                                (int)Enum.Parse(typeof(Borough), feature.Attributes["borough"].ToString()) : 0,

                BoroName = EnumHelper.IsEnumValid<Borough>(feature.Attributes["borough"].ToString()) && feature.Attributes["borough"] != null ?
                                feature.Attributes["borough"].ToString() : null,

                BBL = feature.Attributes["bbl"] != null ? Double.Parse(feature.Attributes["bbl"].ToString()) : 0,
                ShapeArea = double.Parse(feature.Attributes["shape_area"].ToString()),
                ShapeLength = double.Parse(feature.Attributes["shape_leng"].ToString()),
            };
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
                    BoroCode = EnumHelper.IsEnumValid<Borough>(f.Attributes["borough"].ToString()) && f.Attributes["borough"] != null ?
                        (int)Enum.Parse(typeof(Borough), f.Attributes["borough"].ToString()) : 0,
                    BoroName = EnumHelper.IsEnumValid<Borough>(f.Attributes["borough"].ToString()) && f.Attributes["borough"] != null ?
                        f.Attributes["borough"].ToString() : null,
                    BBL = f.Attributes["bbl"] != null ? Double.Parse(f.Attributes["bbl"].ToString()) : 0,
                    ShapeArea = double.Parse(f.Attributes["shape_area"].ToString()),
                    ShapeLength = double.Parse(f.Attributes["shape_leng"].ToString()),
                });

            return results;
        }

    }
}
