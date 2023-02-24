using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace MicroService.Service.Services
{
    public class NychaDevelopmentService<T> : AbstractShapeService<NychaDevelopmentShape>, IShapeService<NychaDevelopmentShape>
    {
        public NychaDevelopmentService(ShapefileDataReaderResolver shapefileDataReaderResolver)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.NychaDevelopments));
        }

        public override NychaDevelopmentShape GetFeatureLookup(double x, double y)
        {
            // Validate Point is in Range
            var point = new Point(x, y);

            var model = new NychaDevelopmentShape();
            var features = GetFeatures();
            foreach (var f in features)
            {
                var exists = f.Geometry.Contains(point);
                if (exists)
                {
                    model = new NychaDevelopmentShape
                    {
                        Development = f.Attributes["DEVELOPMEN"].ToString(),
                        TdsNumber = f.Attributes["TDS_NUM"].ToString(),
                        Borough = f.Attributes["BOROUGH"].ToString(),
                    };
                }
            }

            if (!model.ArePropertiesNotNull())
            {
                return null;
            }
            return model;
        }

        public override IEnumerable<NychaDevelopmentShape> GetFeatureLookup(List<KeyValuePair<string, string>> features)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<NychaDevelopmentShape> GetFeatureAttributes()
        {
            var features = GetFeatures();
            var results = new List<NychaDevelopmentShape>(features.Count);

            foreach (var f in features)
            {
                var model = new NychaDevelopmentShape
                {
                    Development = f.Attributes["DEVELOPMEN"].ToString(),
                    TdsNumber = f.Attributes["TDS_NUM"].ToString(),
                    Borough = f.Attributes["BOROUGH"].ToString(),
                };

                results.Add(model);
            }

            return results;
        }
    }
}
