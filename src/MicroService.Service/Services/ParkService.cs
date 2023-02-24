using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace MicroService.Service.Services
{
    public class ParkService<T> : AbstractShapeService<ParkShape>, IShapeService<ParkShape>
    {
        public ParkService(ShapefileDataReaderResolver shapefileDataReaderResolver)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.Parks));
        }

        public override ParkShape GetFeatureLookup(double x, double y)
        {
            // Validate Point is in Range
            var point = new Point(x, y);

            var model = new ParkShape();

            var features = GetFeatures();
            foreach (var f in features)
            {
                var exists = f.Geometry.Contains(point);
                if (exists)
                {
                    model = new ParkShape
                    {
                        ParkName = f.Attributes["PARK_NAME"].ToString(),
                        ParkNumber = f.Attributes["PARKNUM"].ToString(),
                        SourceId = long.Parse(f.Attributes["SOURCE_ID"].ToString()),
                        FeatureCode = int.Parse(f.Attributes["FEAT_CODE"].ToString()),
                        SubCode = int.Parse(f.Attributes["SUB_CODE"].ToString()),
                        LandUse = f.Attributes["LANDUSE"].ToString(),
                    };
                }
            }

            if (!model.ArePropertiesNotNull())
            {
                return null;
            }

            return model;
        }

        public override IEnumerable<ParkShape> GetFeatureLookup(List<KeyValuePair<string, string>> attributes)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<ParkShape> GetFeatureAttributes()
        {
            var features = GetFeatures();
            var results = new List<ParkShape>(features.Count);

            foreach (var f in features)
            {
                var model = new ParkShape
                {
                    ParkName = f.Attributes["PARK_NAME"].ToString(),
                    ParkNumber = f.Attributes["PARKNUM"].ToString(),
                    SourceId = long.Parse(f.Attributes["SOURCE_ID"].ToString()),
                    FeatureCode = int.Parse(f.Attributes["FEAT_CODE"].ToString()),
                    SubCode = int.Parse(f.Attributes["SUB_CODE"].ToString()),
                    LandUse = f.Attributes["LANDUSE"].ToString(),
                };

                results.Add(model);
            }

            return results;
        }

    }
}
