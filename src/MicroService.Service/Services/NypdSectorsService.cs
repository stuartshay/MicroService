﻿using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using NetTopologySuite.Geometries;

namespace MicroService.Service.Services
{
    public class NypdSectorsService<T> : AbstractShapeService<NypdSectorShape>, IShapeService<NypdSectorShape>
    {
        public NypdSectorsService(ShapefileDataReaderResolver shapefileDataReaderResolver)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.NypdSectors));
        }

        public override NypdSectorShape GetFeatureLookup(double x, double y)
        {
            // Validate Point is in Range
            var point = new Point(x, y);

            var model = new NypdSectorShape();

            var features = GetFeatures();
            foreach (var f in features)
            {
                var exists = f.Geometry.Contains(point);
                if (exists)
                {
                    model = new NypdSectorShape
                    {
                        Pct = f.Attributes["pct"].ToString(),
                        Sector = f.Attributes["sector"].ToString(),
                        PatrolBoro = f.Attributes["patrol_bor"].ToString(),
                    };
                }
            }

            if (!model.ArePropertiesNotNull())
            {
                return null;
            }

            return model;
        }
    }
}
