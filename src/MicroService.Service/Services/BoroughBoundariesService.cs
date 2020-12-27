using System.Collections;
using System.Collections.Generic;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using NetTopologySuite.Geometries;

namespace MicroService.Service.Services
{
    public class BoroughBoundariesService : AbstractShapeService<BoroughBoundaryShape>, IShapeService<BoroughBoundaryShape>
    {
        public BoroughBoundariesService(ShapefileDataReaderResolver shapefileDataReaderResolver)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.BoroughBoundaries));
        }

        public override BoroughBoundaryShape GetFeatureLookup(double x, double y)
        {
            // Validate Point is in Range
            var point = new Point(x, y);

            var model = new BoroughBoundaryShape();

            var features = GetFeatures();
            foreach (var f in features)
            {
                var exists = f.Geometry.Contains(point);
                if (exists)
                {
                    model = new BoroughBoundaryShape
                    {
                        BoroCode = f.Attributes["BoroCode"].ToString(),
                        BoroName = f.Attributes["BoroName"].ToString(),
                    };
                }
            }

            if (!model.ArePropertiesNotNull())
            {
                return null;
            }

            return model;
        }

        public IEnumerable<BoroughBoundaryShape> GetFeatureAttributes()
        {
            var features = GetFeatures();
            var results = new List<BoroughBoundaryShape>(features.Count);

            foreach (var f in features)
            {
                var model = new BoroughBoundaryShape
                {
                    BoroCode = f.Attributes["BoroCode"].ToString(),
                    BoroName = f.Attributes["BoroName"].ToString(),
                };

                results.Add(model);
            }

            return results;
        }

    }
}
