using System.IO;
using MicroService.Service.Configuration;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace MicroService.Service.Services
{
    public class NypdPolicePrecinctService : AbstractShapeService<NypdPrecinctShape>, IShapeService<NypdPrecinctShape>
    {
        public NypdPolicePrecinctService(IOptions<ApplicationOptions> options)
        {
            // Get Shape Properties
            var shapeProperties = ShapeProperties.NypdPolicePrecincts.GetAttribute<ShapeAttributes>();

            var shapeDirectory = $"{Path.Combine(options.Value.ShapeConfiguration.ShapeRootDirectory, shapeProperties.Directory, shapeProperties.FileName)}";
            string shapePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), shapeDirectory));

            GeometryFactory factory = new GeometryFactory();
            _shapeFileDataReader = new ShapefileDataReader(shapePath, factory);
        }

        public override NypdPrecinctShape GetFeatureLookup(double x, double y)
        {
            // Validate Point is in Range
            var point = new Point(x, y);

            var model = new NypdPrecinctShape();

            var features = GetFeatures();
            foreach (var f in features)
            {
                var exists = f.Geometry.Contains(point);
                if (exists)
                {
                    model = new NypdPrecinctShape
                    {
                        Precinct = f.Attributes["Precinct"].ToString(),
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
