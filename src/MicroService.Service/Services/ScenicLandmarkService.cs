using AutoMapper;
using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroService.Service.Services
{
    public class ScenicLandmarkService : AbstractShapeService<ScenicLandmarkShape>, IShapeService<ScenicLandmarkShape>
    {
        public ScenicLandmarkService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<ScenicLandmarkService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.ScenicLandmarks));
        }

        public virtual ScenicLandmarkShape GetFeatureLookup(double x, double y)
        {
            // Convert Nad83 to Wgs 
            var result = GeoTransformationHelper.ConvertNad83ToWgs84(x, y);
            var wgs84Point = new { X = result.Item2, Y = result.Item1 };

            var point = new Point(wgs84Point.Y.Value, wgs84Point.X.Value);

            var features = GetFeatures();
            var feature = features.FirstOrDefault(f => f.Geometry.Contains(point));

            if (feature == null)
            {
                return null;
            }

            return new ScenicLandmarkShape
            {
                LPNumber = feature.Attributes["lp_number"].ToString(),
                AreaName = feature.Attributes["scen_lm_na"].ToString(),
                BoroName = feature.Attributes["borough"].ToString(),
                BoroCode = (int)Enum.Parse(typeof(Borough), feature.Attributes["borough"].ToString()),
                ShapeArea = double.Parse(feature.Attributes["shape_area"].ToString()),
                ShapeLength = double.Parse(feature.Attributes["shape_leng"].ToString()),
            };
        }

        public IEnumerable<ScenicLandmarkShape> GetFeatureLookup(List<KeyValuePair<string, object>> attributes)
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
                .Select(f => new ScenicLandmarkShape
                {
                    LPNumber = f.Attributes["lp_number"].ToString(),
                    AreaName = f.Attributes["scen_lm_na"].ToString(),
                    BoroName = f.Attributes["borough"].ToString(),
                    BoroCode = (int)Enum.Parse(typeof(Borough), f.Attributes["borough"].ToString()),
                    ShapeArea = double.Parse(f.Attributes["shape_area"].ToString()),
                    ShapeLength = double.Parse(f.Attributes["shape_leng"].ToString()),
                    BoundingBox = !f.BoundingBox.IsNull
                        ? new BoundingBox
                        {
                            Area = f.BoundingBox.Area,
                            Centre = f.BoundingBox.Centre.IsValid
                                    ? new CentrePoint { X = f.BoundingBox.Centre.X, Y = f.BoundingBox.Centre.Y, }
                                    : null,
                            Diameter = f.BoundingBox.Diameter,
                            MaxX = f.BoundingBox.MaxX,
                            MaxY = f.BoundingBox.MaxY,
                            MinX = f.BoundingBox.MinX,
                            MinY = f.BoundingBox.MinY,
                            MinExtent = f.BoundingBox.MinExtent,
                            MaxExtent = f.BoundingBox.MaxExtent,
                        }
                        : null,
                    Geometry = f.Geometry,
                });

            return results;
        }

        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            attributes = ValidateFeatureKey(attributes);
            var featureCollection = new FeatureCollection();

            var features = GetFeatures()
                .Where(f => attributes.All(pair =>
                {
                    var value = f.Attributes[pair.Key];
                    var expectedValue = pair.Value;
                    var matchedValue = MatchAttributeValue(value, expectedValue);
                    return matchedValue != null;
                }))
                .Select(f => new ScenicLandmarkShape
                {
                    LPNumber = f.Attributes["lp_number"].ToString(),
                    AreaName = f.Attributes["scen_lm_na"].ToString(),
                    BoroName = f.Attributes["borough"].ToString(),
                    BoroCode = (int)Enum.Parse(typeof(Borough), f.Attributes["borough"].ToString()),
                    ShapeArea = double.Parse(f.Attributes["shape_area"].ToString()),
                    ShapeLength = double.Parse(f.Attributes["shape_leng"].ToString()),
                    Geometry = f.Geometry,
                });

            foreach (var feature in features)
            {
                var featureProperties = EnumHelper.GetPropertiesWithoutExcludedAttribute<ScenicLandmarkShape, FeatureCollectionExcludeAttribute>();
                var featureAttributes = featureProperties
                    .ToDictionary(prop => prop.Name, prop => prop.GetValue(feature, null));

                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }

        public IEnumerable<ScenicLandmarkShape> GetFeatureList()
        {
            var features = GetFeatures();

            return features.Select(f => new ScenicLandmarkShape
            {
                LPNumber = f.Attributes["lp_number"].ToString(),
                AreaName = f.Attributes["scen_lm_na"].ToString(),
                BoroName = f.Attributes["borough"].ToString(),
                BoroCode = (int)Enum.Parse(typeof(Borough), f.Attributes["borough"].ToString()),
                ShapeArea = double.Parse(f.Attributes["shape_area"].ToString()),
                ShapeLength = double.Parse(f.Attributes["shape_leng"].ToString()),
            });
        }

    }
}
