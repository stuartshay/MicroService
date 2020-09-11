using System.Collections.Generic;
using NetTopologySuite.Features;
using NetTopologySuite.IO;

namespace MicroService.Service.Services
{
    public abstract class AbstractShapeService
    {
        public ShapefileDataReader _shapeFileDataReader { get; set; }

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
    }
}
