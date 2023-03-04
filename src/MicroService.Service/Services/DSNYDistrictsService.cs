using AutoMapper;
using MicroService.Data.Enum;
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
    public class DsnyDistrictsService : AbstractShapeService<DsnyDistrictsShape>, IShapeService<DsnyDistrictsShape>
    {
        public DsnyDistrictsService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<DsnyDistrictsService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.DSNYDistricts));
        }

        public virtual DsnyDistrictsShape GetFeatureLookup(double x, double y)
        {
            // Convert Nad83 to Wgs 
            var result = GeoTransformationHelper.ConvertNad83ToWgs84(x, y);
            var wgs84Point = new { X = result.Item1, Y = result.Item2 };

            // Validate Point is in Range
            var point = new Point(wgs84Point.X.Value, wgs84Point.Y.Value);

            var features = GetFeatures();
            var feature = features.FirstOrDefault(f => f.Geometry.Contains(point));

            if (feature == null)
            {
                return null;
            }

            return new DsnyDistrictsShape
            {
                District = feature.Attributes["district"].ToString(),
                DistrictCode = int.Parse(feature.Attributes["districtco"].ToString()),
                OperationZone = feature.Attributes["district"].ToString().RemoveIntegers(),
                OperationZoneName = EnumHelper.ParseEnum<DsnyOperationZone>(feature.Attributes["district"].ToString().RemoveIntegers()).GetEnumDescription(),
                ShapeArea = double.Parse(feature.Attributes["shape_area"].ToString()),
                ShapeLength = double.Parse(feature.Attributes["shape_leng"].ToString()),
            };
        }

        public IEnumerable<DsnyDistrictsShape> GetFeatureLookup(List<KeyValuePair<string, object>> attributes)
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
                .Select(f => new DsnyDistrictsShape
                {
                    District = f.Attributes["district"].ToString(),
                    DistrictCode = int.Parse(f.Attributes["districtco"].ToString()),
                    OperationZone = f.Attributes["district"].ToString().RemoveIntegers(),
                    OperationZoneName = f.Attributes["district"].ToString().RemoveIntegers().ParseEnum<DsnyOperationZone>().GetEnumDescription(),
                    ShapeArea = double.Parse(f.Attributes["shape_area"].ToString()),
                    ShapeLength = double.Parse(f.Attributes["shape_leng"].ToString()),
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
                .Select(f => new DsnyDistrictsShape
                {
                    District = f.Attributes["district"].ToString(),
                    DistrictCode = int.Parse(f.Attributes["districtco"].ToString()),
                    OperationZone = f.Attributes["district"].ToString().RemoveIntegers(),
                    OperationZoneName = f.Attributes["district"].ToString().RemoveIntegers().ParseEnum<DsnyOperationZone>().GetEnumDescription(),
                    ShapeArea = double.Parse(f.Attributes["shape_area"].ToString()),
                    ShapeLength = double.Parse(f.Attributes["shape_leng"].ToString()),
                    Geometry = f.Geometry,
                });

            foreach (var feature in features)
            {
                var featureProperties = EnumHelper.GetPropertiesWithoutExcludedAttribute<DsnyDistrictsShape, FeatureCollectionExcludeAttribute>();
                var featureAttributes = featureProperties
                    .ToDictionary(prop => prop.Name, prop => prop.GetValue(feature, null));

                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }

        public IEnumerable<DsnyDistrictsShape> GetFeatureList()
        {
            var features = GetFeatures();

            var districts = features.Select(f => new
            {
                District = f.Attributes["district"].ToString(),
                DistrictCode = int.Parse(f.Attributes["districtco"].ToString()),
                OperationZone = f.Attributes["district"].ToString().RemoveIntegers(),
                ShapeArea = double.Parse(f.Attributes["shape_area"].ToString()),
                ShapeLength = double.Parse(f.Attributes["shape_leng"].ToString())
            }).ToList();

            return districts.Select(d => new DsnyDistrictsShape
            {
                District = d.District,
                DistrictCode = d.DistrictCode,
                OperationZone = d.OperationZone,
                OperationZoneName = d.OperationZone.ParseEnum<DsnyOperationZone>().GetEnumDescription(),
                ShapeArea = d.ShapeArea,
                ShapeLength = d.ShapeLength
            }).OrderBy(x => x.District).ToList();
        }
    }
}