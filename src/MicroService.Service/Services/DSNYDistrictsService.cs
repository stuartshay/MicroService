using MicroService.Data.Enum;
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
    public class DsnyDistrictsService : AbstractShapeService<DsnyDistrictsShape>, IShapeService<DsnyDistrictsShape>
    {
        public DsnyDistrictsService(ShapefileDataReaderResolver shapefileDataReaderResolver)
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

        public override IEnumerable<DsnyDistrictsShape> GetFeatureLookup(List<KeyValuePair<string, string>> attributes)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<DsnyDistrictsShape> GetFeatureAttributes()
        {
            var features = GetFeatures();

            return features.Select(f =>
                {
                    var district = f.Attributes["district"].ToString();
                    var operationZone = district.RemoveIntegers();
                    var operationZoneName = operationZone.ParseEnum<DsnyOperationZone>().GetEnumDescription();

                    return new DsnyDistrictsShape
                    {
                        District = district,
                        DistrictCode = int.Parse(f.Attributes["districtco"].ToString()),
                        OperationZone = operationZone,
                        OperationZoneName = operationZoneName,
                        ShapeArea = double.Parse(f.Attributes["shape_area"].ToString()),
                        ShapeLength = double.Parse(f.Attributes["shape_leng"].ToString()),
                    };
                })
                .OrderBy(x => x.District)
                .ToList();
        }
    }
}