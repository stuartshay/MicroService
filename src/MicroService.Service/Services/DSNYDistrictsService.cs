using System.Collections.Generic;
using System.Linq;
using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using NetTopologySuite.Geometries;

namespace MicroService.Service.Services
{
    public class DSNYDistrictsService : AbstractShapeService<DSNYDistrictsShape>, IShapeService<DSNYDistrictsShape>
    {
        public DSNYDistrictsService(ShapefileDataReaderResolver shapefileDataReaderResolver)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.DSNYDistricts));
        }

        public override DSNYDistrictsShape GetFeatureLookup(double x, double y)
        {
            // Convert Nad83 to Wgs 
            var result = GeoTransformationHelper.ConvertNad83ToWgs84(x, y);
            var wgs84Point = new { X = result.Item1, Y = result.Item2 };

            // Validate Point is in Range
            var point = new Point(wgs84Point.X.Value, wgs84Point.Y.Value);

            var model = new DSNYDistrictsShape();

            var features = GetFeatures();
            foreach (var f in features)
            {
                var exists = f.Geometry.Contains(point);
                if (exists)
                {
                    var district = f.Attributes["district"].ToString();
                    var operationZone = district.RemoveIntegers();
                    var operationZoneName = EnumHelper.ParseEnum<DsnyOperationZone>(operationZone).GetEnumDescription();

                    model = new DSNYDistrictsShape
                    {
                        District = district,
                        DistrictCode = f.Attributes["districtco"].ToString(),
                        OperationZone = operationZone,
                        OperationZoneName = operationZoneName
                    };
                }
            }

            if (!model.ArePropertiesNotNull())
            {
                return null;
            }

            return model;
        }

        public IEnumerable<DSNYDistrictsShape> GetFeatureAttributes()
        {
            var features = GetFeatures();
            var results = new List<DSNYDistrictsShape>(features.Count);

            foreach (var f in features)
            {
                var district = f.Attributes["district"].ToString();
                var operationZone = district.RemoveIntegers();
                var operationZoneName = EnumHelper.ParseEnum<DsnyOperationZone>(operationZone).GetEnumDescription();
                
                var model = new DSNYDistrictsShape
                {
                    District = district,
                    DistrictCode = f.Attributes["districtco"].ToString(),
                    OperationZone = operationZone,
                    OperationZoneName = operationZoneName,
                };

                results.Add(model);
            }

            return results.ToList().OrderBy(x => x.District);
        }
    }
}
