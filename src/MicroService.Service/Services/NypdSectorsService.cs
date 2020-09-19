using System;
using System.IO;
using MicroService.Service.Configuration;
using MicroService.Service.Helpers;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace MicroService.Service.Services
{
    public class NypdSectorsService<T> : AbstractShapeService<NypdSectors>, INypdSectorsService
    {
        public NypdSectorsService(IOptions<ApplicationOptions> options)
        {
            // Get Shape Properties
            Type typeParameterType = typeof(T);
            var name = typeParameterType.Name;
            var shapeProperties = ShapeProperties.NypdSectors.GetAttribute<ShapeAttributes>();

            var shapeDirectory = $"{Path.Combine(options.Value.ShapeConfiguration.ShapeRootDirectory, shapeProperties.Directory, shapeProperties.FileName)}";
            string shapePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), shapeDirectory));

            GeometryFactory factory = new GeometryFactory();
            _shapeFileDataReader = new ShapefileDataReader(shapePath, factory);
        }

        public override NypdSectors GetFeatureLookup(double x, double y)
        {
            // Validate Point is in Range
            var point = new Point(x, y);

            var model = new NypdSectors();

            var features = GetFeatures();
            foreach (var f in features)
            {
                var exists = f.Geometry.Contains(point);
                if (exists)
                {
                    model = new NypdSectors
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
