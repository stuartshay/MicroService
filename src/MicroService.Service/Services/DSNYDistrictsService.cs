using AutoMapper;
using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;
using Coordinate = MicroService.Service.Models.Base.Coordinate;

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

        public override DsnyDistrictsShape GetFeatureLookup(double x, double y)
        {
            // Convert Nad83 to Wgs 
            var result = GeoTransformationHelper.ConvertNad83ToWgs84(x, y);
            var wgs84Point = new { X = result.Item1, Y = result.Item2 };

            // Validate Point is in Range
            var point = new Point(wgs84Point.X.Value, wgs84Point.Y.Value);

            var model = new DsnyDistrictsShape();

            var features = GetFeatures();
            foreach (var f in features)
            {
                var exists = f.Geometry.Contains(point);
                if (exists)
                {
                    var district = f.Attributes["district"].ToString();
                    var operationZone = district.RemoveIntegers();
                    var operationZoneName = EnumHelper.ParseEnum<DsnyOperationZone>(operationZone).GetEnumDescription();

                    model = new DsnyDistrictsShape
                    {
                        District = district,
                        DistrictCode = int.Parse(f.Attributes["districtco"].ToString()),
                        OperationZone = operationZone,
                        OperationZoneName = operationZoneName,
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

        public override IEnumerable<DsnyDistrictsShape> GetFeatureLookup(List<KeyValuePair<string, object>> attributes)
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
                    Coordinates = new List<Coordinate>(),
                });

            return results;
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