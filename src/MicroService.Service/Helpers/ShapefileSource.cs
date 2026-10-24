using MicroService.Service.Models;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Esri;
using NetTopologySuite.IO.Esri.Dbf;
using NetTopologySuite.IO.Esri.Dbf.Fields;
using NetTopologySuite.IO.Esri.Shapefiles.Readers;
using System.Globalization;
using System.Text;

// cspell:ignore Doña

namespace MicroService.Service.Helpers
{
    /// <summary>Reads Esri shapefiles while preserving the service's existing feature contract.</summary>
    public static class ShapefileSource
    {
        static ShapefileSource()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public static (ShapefileProperties Shape, ShapeDatabaseProperties Database) ReadMetadata(string fileName)
        {
            using var dbf = OpenDatabase(fileName);
            using var reader = OpenReader(fileName, dbf.Encoding);
            return (
                new ShapefileProperties { Bounds = new Envelope(reader.BoundingBox), ShapeType = reader.ShapeType.ToString() },
                new ShapeDatabaseProperties
                {
                    NumRecords = dbf.RecordCount,
                    LastUpdateDate = dbf.LastUpdateDate,
                    Fields = dbf.Fields.Select(ToField).ToArray()
                });
        }

        public static IReadOnlyCollection<Feature> ReadFeatures(string fileName)
        {
            using var dbf = OpenDatabase(fileName);
            using var reader = OpenReader(fileName, dbf.Encoding);
            var features = new List<Feature>();
            while (reader.Read())
            {
                var geometry = PreserveGeometry(reader.Geometry);
                var attributes = new AttributesTable();
                foreach (var field in reader.Fields)
                {
                    attributes.Add(field.Name, PreserveValue(field));
                }
                features.Add(new Feature(geometry, attributes) { BoundingBox = geometry.EnvelopeInternal });
            }
            return features;
        }

        private static DbfReader OpenDatabase(string fileName)
        {
            // These versioned archives historically default to Windows-1252. The new
            // reader defaults to UTF-8, which corrupts the Parks dataset's "Doña".
            // A dataset's explicit .cpg encoding still takes precedence.
            var encoding = File.Exists(Path.ChangeExtension(fileName, ".cpg")) ? null : Encoding.GetEncoding(1252);
            return new DbfReader(Path.ChangeExtension(fileName, ".dbf"), encoding);
        }

        private static ShapefileReader OpenReader(string fileName, Encoding encoding) =>
            Shapefile.OpenRead(Path.ChangeExtension(fileName, ".shp"), new ShapefileReaderOptions
            {
                Factory = new GeometryFactory(),
                Encoding = encoding,
                // Preserve existing invalid geometries rather than rejecting, repairing
                // or silently dropping records from the building dataset.
                GeometryBuilderMode = GeometryBuilderMode.IgnoreInvalidShapes
            });

        private static Geometry PreserveGeometry(Geometry geometry)
        {
            if (geometry is not MultiPolygon multi)
            {
                return geometry;
            }

            // The legacy reader orders polygon shells by ascending area. Preserve the
            // coordinate order, holes and single-Polygon GeoJSON representation.
            var polygons = multi.Geometries.Cast<Polygon>().ToList();
            polygons.Sort((a, b) => ShellArea(a).CompareTo(ShellArea(b)));
            return polygons.Count switch
            {
                0 => multi.Factory.CreatePolygon(),
                1 => polygons[0],
                _ => multi.Factory.CreateMultiPolygon(polygons.ToArray())
            };
        }

        private static double ShellArea(Polygon polygon) => Math.Abs(Area.OfRing(polygon.ExteriorRing.Coordinates));

        private static object? PreserveValue(DbfField field)
        {
            var value = field.Value;
            if (field.FieldType is DbfType.Numeric or DbfType.Float)
            {
                // Legacy DBF values were doubles even when metadata reported Int32.
                return value == null ? null : double.Parse(Convert.ToString(value, CultureInfo.InvariantCulture)!, CultureInfo.InvariantCulture);
            }
            if (field.FieldType == DbfType.Character)
            {
                // All-zero DBF text was a fixed-width NUL string; some mappers distinguish
                // that from null. Keep it until the API explicitly changes that contract.
                var text = value == null ? new string('\0', field.Length) : ((string)value).Trim();
                return string.IsNullOrWhiteSpace(text) ? null : text;
            }
            return value;
        }

        private static ShapeField ToField(DbfField field)
        {
            var type = field.FieldType switch
            {
                DbfType.Character => typeof(string),
                DbfType.Date => typeof(DateTime),
                DbfType.Logical => typeof(bool),
                DbfType.Float => typeof(float),
                DbfType.Numeric when field.NumericScale != 0 => typeof(double),
                DbfType.Numeric when field.Length <= 10 => typeof(int),
                DbfType.Numeric => typeof(long),
                _ => throw new NotSupportedException($"Unsupported shape field type: {field.FieldType}")
            };
            return new ShapeField(field.Name, type, (char)field.FieldType, field.Length, field.NumericScale);
        }
    }
}
