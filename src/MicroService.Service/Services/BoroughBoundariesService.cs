using System;
using System.IO;
using MicroService.Service.Configuration;
using MicroService.Service.Models;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace MicroService.Service.Services
{
    public class BoroughBoundariesService : AbstractShapeService, IBoroughBoundariesService
    {
        public BoroughBoundariesService(IOptions<ApplicationOptions> options)
        {
            var shapeDirectory = $"{Path.Combine(options.Value.ShapeConfiguration.ShapeRootDirectory, "Borough_Boundaries","nybb")}";
            
            Console.WriteLine(shapeDirectory);

            string shapePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), shapeDirectory));

            GeometryFactory factory = new GeometryFactory();
            _shapeFileDataReader = new ShapefileDataReader(shapePath, factory);
        }

        public BoroughBoundaries GetFeatureLookup(double x, double y)
        {
            var point = new Point(x, y);

            var model = new BoroughBoundaries();
            var features = GetFeatures();
            foreach (var f in features)
            {
                var z = f.Geometry.Contains(point);
                if (z)
                {
                    model = new BoroughBoundaries
                    {
                        BoroCode = f.Attributes["BoroCode"].ToString(),
                        BoroName = f.Attributes["BoroName"].ToString(),
                    };
                }
            }

            return model;
        }

    }
}
