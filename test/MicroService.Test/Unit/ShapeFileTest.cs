using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Xunit;
using Xunit.Abstractions;

namespace MicroService.Test.Unit
{
    public class ShapeFileTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ShapeFileTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [InlineData(@"NYPD_Sectors\NYPD_Sectors")]
        [Theory(DisplayName = "Shape OutputFeatures")]
        public void Shape_OutputFeatures(string shapeFilePath)
        {
            string shapeDirectory = $"../../../../../files/{shapeFilePath}";
            string shapePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), shapeDirectory));

            GeometryFactory factory = new();
            ShapefileDataReader shapeFileDataReader = new(shapePath, factory);

            ShapefileHeader shpHeader = shapeFileDataReader.ShapeHeader;
            _testOutputHelper.WriteLine($"Shape type: {shpHeader.ShapeType}");

            //Display the min and max bounds of the shapefile
            var bounds = shpHeader.Bounds;
            _testOutputHelper.WriteLine($"Min bounds: ({bounds.MinX},{bounds.MinY})");
            _testOutputHelper.WriteLine($"Max bounds: ({bounds.MaxX},{bounds.MaxY})");

            //Display summary information about the Dbase file
            DbaseFileHeader header = shapeFileDataReader.DbaseHeader;
            _testOutputHelper.WriteLine("Dbase info");
            _testOutputHelper.WriteLine($"{header.Fields.Length} Columns, {header.NumRecords} Records");

            for (int i = 0; i < header.NumFields; i++)
            {
                DbaseFieldDescriptor fldDescriptor = header.Fields[i];
                _testOutputHelper.WriteLine($"   {fldDescriptor.Name} {fldDescriptor.DbaseType}");
            }

            // Read through all records of the shapefile (geometry and attributes) into a feature collection
            var features = new List<Feature>();
            while (shapeFileDataReader.Read())
            {
                Feature feature = new();
                AttributesTable attributesTable = new();

                string[] keys = new string[header.NumFields];
                var geometry = shapeFileDataReader.Geometry;

                for (int i = 0; i < header.NumFields; i++)
                {
                    DbaseFieldDescriptor fldDescriptor = header.Fields[i];
                    keys[i] = fldDescriptor.Name;

                    // First Field Geometry
                    var value = shapeFileDataReader.GetValue(i + 1);
                    attributesTable.Add(fldDescriptor.Name, value);
                }

                feature.Geometry = geometry;
                feature.Attributes = attributesTable;
                features.Add(feature);
            }

            var result = features;
            foreach (var f in features)
            {
                var z = f.Geometry.Contains(new Point(1032999, 217570));
                if (z)
                {
                    var z1 = f.Attributes["pct"];
                    // var z2 = f.Attributes["patrol_bor"];
                    // var z3 = f.Attributes["sector"];
                }
            }
        }

        [Fact]
        public void Test_WKTReader()
        {
            WktReaderFunctions.GeometryContainsPoint();
            Assert.True(true);
        }

    }
}
