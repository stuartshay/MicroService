using System.Collections.Generic;
using NetTopologySuite.Features;
using NetTopologySuite.IO;

namespace MicroService.Service.Extensions
{
    public static class ShapefileDataReaderExtensions
    {
        public static IReadOnlyCollection<Feature> ReadFeatures(this ShapefileDataReader shapefileDataReader)
        {
            List<Feature> features = new List<Feature>();
            while (shapefileDataReader.Read())
            {
                Feature feature = new Feature();
                AttributesTable attributesTable = new AttributesTable();
                DbaseFileHeader header = shapefileDataReader.DbaseHeader;

                string[] keys = new string[header.NumFields];
                var geometry = shapefileDataReader.Geometry;

                for (int i = 0; i < header.NumFields; i++)
                {
                    DbaseFieldDescriptor fldDescriptor = header.Fields[i];
                    keys[i] = fldDescriptor.Name;

                    // First Field Geometry
                    var value = shapefileDataReader.GetValue(i + 1);
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
