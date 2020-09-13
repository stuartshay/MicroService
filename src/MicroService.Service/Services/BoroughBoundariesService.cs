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
    public class BoroughBoundariesService : AbstractShapeService<BoroughBoundaries>, IBoroughBoundariesService
    {
        public BoroughBoundariesService(IOptions<ApplicationOptions> options)
        {
            // Get Shape Properties
            var shapeProperties = ShapeProperties.BoroughBoundaries.GetAttribute<ShapeAttributes>();
            
            var shapeDirectory = $"{Path.Combine(options.Value.ShapeConfiguration.ShapeRootDirectory, shapeProperties.Directory, shapeProperties.FileName)}";
            string shapePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), shapeDirectory));

            GeometryFactory factory = new GeometryFactory();
            _shapeFileDataReader = new ShapefileDataReader(shapePath, factory);
        }

        public override BoroughBoundaries GetFeatureLookup(double x, double y)
        {
            // Validate Point is in Range
            var point = new Point(x, y);

            var model = new BoroughBoundaries();
            
            var features = GetFeatures();
            foreach (var f in features)
            {
                var exists = f.Geometry.Contains(point);
                if (exists)
                {
                    model = new BoroughBoundaries
                    {
                        BoroCode = f.Attributes["BoroCode"].ToString(),
                        BoroName = f.Attributes["BoroName"].ToString(),
                    };
                }

            }

            if(!model.ArePropertiesNotNull())
            {
                return null;
            }

            return model;
        }

    }
}
