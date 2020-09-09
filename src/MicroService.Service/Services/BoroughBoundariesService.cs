using System.Collections.Generic;
using System.IO;
using MicroService.Service.Configuration;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace MicroService.Service.Services
{
    public class BoroughBoundariesService : IBoroughBoundariesService
    {
        private readonly ShapefileDataReader _shapeFileDataReader;

        public BoroughBoundariesService(IOptions<ApplicationOptions> options)
        {
            var shapeDirectory = $"{options.Value.ShapeConfiguration.ShapeRootDirectory}\\Borough_Boundaries\\nybb";
            string shapePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), shapeDirectory));

            GeometryFactory factory = new GeometryFactory();
            _shapeFileDataReader = new ShapefileDataReader(shapePath, factory);

        }

        public List<Feature> GetFeatures()
        {
            var features = new List<Feature>();
            while (_shapeFileDataReader.Read())
            {
                Feature feature = new Feature();
                AttributesTable attributesTable = new AttributesTable();
                DbaseFileHeader header = _shapeFileDataReader.DbaseHeader;

                string[] keys = new string[header.NumFields];
                var geometry = _shapeFileDataReader.Geometry;

                for (int i = 0; i < header.NumFields; i++)
                {
                    DbaseFieldDescriptor fldDescriptor = header.Fields[i];
                    keys[i] = fldDescriptor.Name;

                    // First Field Geometry
                    var value = _shapeFileDataReader.GetValue(i + 1);
                    attributesTable.Add(fldDescriptor.Name, value);
                }

                feature.Geometry = geometry;
                feature.Attributes = attributesTable;
                features.Add(feature);
            }

            return features;
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

        public ShapefileHeader GetShapeProperties()
        {
            ShapefileHeader shpHeader = _shapeFileDataReader.ShapeHeader;
            return shpHeader;
        }

        public DbaseFileHeader GetShapeDatabaseProperties()
        {
            DbaseFileHeader header = _shapeFileDataReader.DbaseHeader;
            return header;
        }
    }
}
